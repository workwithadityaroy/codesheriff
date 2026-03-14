'use client';

import { useState, useEffect, useCallback } from 'react';
import { useAuth } from '@clerk/nextjs';
import { UserPlus, Users, Loader2 } from 'lucide-react';
import { MemberDto } from '@/types/api';
import MembersTable from './MembersTable';
import InviteModal from './InviteModal';

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000';

interface Props {
  repoId: string;
  ownerClerkUserId: string;
  currentClerkUserId: string;
}

export default function TeamTab({ repoId, ownerClerkUserId, currentClerkUserId }: Props) {
  const { getToken } = useAuth();
  const [members, setMembers] = useState<MemberDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [showInvite, setShowInvite] = useState(false);

  const isOwner = currentClerkUserId === ownerClerkUserId;

  const loadMembers = useCallback(async () => {
    try {
      const token = await getToken();
      const res = await fetch(`${API_URL}/api/v1/repositories/${repoId}/members`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (res.ok) setMembers(await res.json());
    } finally {
      setLoading(false);
    }
  }, [repoId, getToken]);

  useEffect(() => { loadMembers(); }, [loadMembers]);

  if (loading) {
    return (
      <div className="flex items-center justify-center py-16">
        <Loader2 size={20} className="animate-spin text-neutral-500" />
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <Users size={15} className="text-neutral-500" />
          <span className="text-sm text-neutral-400">{members.length} member{members.length !== 1 ? 's' : ''}</span>
        </div>
        {isOwner && (
          <button
            onClick={() => setShowInvite(true)}
            className="flex items-center gap-2 px-3 py-1.5 rounded-lg text-xs font-medium bg-blue-600 hover:bg-blue-500 text-white transition-colors"
          >
            <UserPlus size={13} />
            Invite Member
          </button>
        )}
      </div>

      {members.length === 0 ? (
        <div className="rounded-xl border border-neutral-800 bg-neutral-900/30 py-12 text-center">
          <Users size={22} className="mx-auto mb-3 text-neutral-600" />
          <p className="text-sm text-neutral-500">No team members yet.</p>
          {isOwner && (
            <p className="mt-1 text-xs text-neutral-600">Invite your team to collaborate on code reviews.</p>
          )}
        </div>
      ) : (
        <MembersTable
          repoId={repoId}
          members={members}
          isOwner={isOwner}
          onRefresh={loadMembers}
        />
      )}

      {showInvite && (
        <InviteModal
          repoId={repoId}
          onClose={() => setShowInvite(false)}
          onSuccess={() => {
            loadMembers();
          }}
        />
      )}
    </div>
  );
}
