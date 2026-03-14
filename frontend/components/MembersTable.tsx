'use client';

import { useState } from 'react';
import { useAuth } from '@clerk/nextjs';
import { UserCheck, UserX, Clock, Trash2, Loader2 } from 'lucide-react';
import { MemberDto } from '@/types/api';

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000';

interface Props {
  repoId: string;
  members: MemberDto[];
  isOwner: boolean;
  onRefresh: () => void;
}

const ROLE_COLORS: Record<string, string> = {
  Owner: 'text-blue-400 bg-blue-500/10 border-blue-500/20',
  Reviewer: 'text-emerald-400 bg-emerald-500/10 border-emerald-500/20',
  Viewer: 'text-neutral-400 bg-neutral-500/10 border-neutral-500/20',
};

export default function MembersTable({ repoId, members, isOwner, onRefresh }: Props) {
  const { getToken } = useAuth();
  const [removing, setRemoving] = useState<string | null>(null);

  const handleRemove = async (memberId: string) => {
    if (!confirm('Remove this member from the repository?')) return;
    setRemoving(memberId);
    try {
      const token = await getToken();
      const res = await fetch(`${API_URL}/api/v1/repositories/${repoId}/members/${memberId}`, {
        method: 'DELETE',
        headers: { Authorization: `Bearer ${token}` },
      });
      if (res.ok) onRefresh();
    } finally {
      setRemoving(null);
    }
  };

  return (
    <div className="overflow-hidden rounded-xl border border-neutral-800">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-neutral-800 bg-neutral-900/50">
            <th className="px-4 py-3 text-left text-[11px] font-semibold uppercase tracking-widest text-neutral-500">Member</th>
            <th className="px-4 py-3 text-left text-[11px] font-semibold uppercase tracking-widest text-neutral-500">Role</th>
            <th className="px-4 py-3 text-left text-[11px] font-semibold uppercase tracking-widest text-neutral-500">Status</th>
            {isOwner && <th className="px-4 py-3 w-12" />}
          </tr>
        </thead>
        <tbody className="divide-y divide-neutral-800/60">
          {members.map(member => (
            <tr key={member.id || 'owner'} className="hover:bg-neutral-900/30 transition-colors">
              <td className="px-4 py-3 text-neutral-300">
                {member.invitedEmail || (
                  <span className="text-neutral-500 italic">you</span>
                )}
              </td>
              <td className="px-4 py-3">
                <span className={`inline-flex items-center px-2 py-0.5 rounded-md text-[11px] font-semibold border ${ROLE_COLORS[member.role] ?? ROLE_COLORS.Viewer}`}>
                  {member.role}
                </span>
              </td>
              <td className="px-4 py-3">
                {member.isAccepted ? (
                  <span className="flex items-center gap-1.5 text-emerald-400 text-xs">
                    <UserCheck size={13} /> Accepted
                  </span>
                ) : (
                  <span className="flex items-center gap-1.5 text-amber-400 text-xs">
                    <Clock size={13} /> Pending
                  </span>
                )}
              </td>
              {isOwner && (
                <td className="px-4 py-3 text-right">
                  {member.id !== '00000000-0000-0000-0000-000000000000' && (
                    <button
                      onClick={() => handleRemove(member.id)}
                      disabled={removing === member.id}
                      className="p-1.5 rounded-lg text-neutral-600 hover:text-red-400 hover:bg-red-500/10 disabled:opacity-50 transition-colors"
                    >
                      {removing === member.id
                        ? <Loader2 size={13} className="animate-spin" />
                        : <Trash2 size={13} />}
                    </button>
                  )}
                </td>
              )}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
