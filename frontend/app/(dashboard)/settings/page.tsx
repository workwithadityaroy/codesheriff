'use client';

import { useState, useEffect, useCallback } from 'react';
import { useAuth } from '@clerk/nextjs';
import { Settings, Key, Bell, CheckCircle, XCircle, Loader2, Eye, EyeOff } from 'lucide-react';
import { UserSettingsDto } from '@/types/api';

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000';

const AI_PROVIDERS = [
  { value: 'claude', label: 'Claude (Anthropic)', placeholder: 'claude-haiku-4-5-20251001' },
  { value: 'openai', label: 'OpenAI', placeholder: 'gpt-4o-mini' },
  { value: 'azure-openai', label: 'Azure OpenAI', placeholder: 'gpt-4o' },
];

type ToastType = 'success' | 'error';
interface ToastState { message: string; type: ToastType }

export default function SettingsPage() {
  const { getToken } = useAuth();

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [testing, setTesting] = useState(false);
  const [toast, setToast] = useState<ToastState | null>(null);

  // AI Provider fields
  const [aiProvider, setAiProvider] = useState('claude');
  const [aiApiKey, setAiApiKey] = useState('');
  const [aiModel, setAiModel] = useState('');
  const [showApiKey, setShowApiKey] = useState(false);

  // Notification fields
  const [notificationEmail, setNotificationEmail] = useState('');
  const [weeklyReportEnabled, setWeeklyReportEnabled] = useState(true);

  const showToast = useCallback((message: string, type: ToastType) => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 4000);
  }, []);

  const authFetch = useCallback(async (path: string, options?: RequestInit) => {
    const token = await getToken();
    return fetch(`${API_URL}${path}`, {
      ...options,
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json',
        ...(options?.headers ?? {}),
      },
    });
  }, [getToken]);

  useEffect(() => {
    authFetch('/api/v1/settings')
      .then(r => r.json() as Promise<UserSettingsDto>)
      .then(data => {
        setAiProvider(data.aiProvider ?? 'claude');
        setAiModel(data.aiModel ?? '');
        setNotificationEmail(data.notificationEmail ?? '');
        setWeeklyReportEnabled(data.weeklyReportEnabled ?? true);
      })
      .catch(() => showToast('Failed to load settings.', 'error'))
      .finally(() => setLoading(false));
  }, [authFetch, showToast]);

  const handleSave = async () => {
    setSaving(true);
    try {
      const res = await authFetch('/api/v1/settings', {
        method: 'PUT',
        body: JSON.stringify({
          aiProvider,
          aiModel,
          aiApiKey,
          notificationEmail,
          weeklyReportEnabled,
        }),
      });
      if (res.ok) {
        showToast('Settings saved successfully.', 'success');
        setAiApiKey(''); // clear key from UI after save
      } else {
        const body = await res.json().catch(() => ({}));
        showToast(body?.error ?? 'Failed to save settings.', 'error');
      }
    } catch {
      showToast('Network error. Please try again.', 'error');
    } finally {
      setSaving(false);
    }
  };

  const handleTestConnection = async () => {
    if (!aiApiKey) {
      showToast('Enter an API key to test the connection.', 'error');
      return;
    }
    setTesting(true);
    try {
      const res = await authFetch('/api/v1/settings/test-connection', {
        method: 'POST',
        body: JSON.stringify({ aiProvider, aiApiKey, aiModel }),
      });
      if (res.ok) {
        showToast('Connection successful!', 'success');
      } else {
        const body = await res.json().catch(() => ({}));
        showToast(body?.error ?? 'Connection failed.', 'error');
      }
    } catch {
      showToast('Network error. Please try again.', 'error');
    } finally {
      setTesting(false);
    }
  };

  const selectedProvider = AI_PROVIDERS.find(p => p.value === aiProvider);

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <Loader2 size={22} className="animate-spin text-neutral-500" />
      </div>
    );
  }

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      {/* Toast */}
      {toast && (
        <div className={`fixed top-5 right-5 z-50 flex items-center gap-2.5 px-4 py-3 rounded-lg text-sm font-medium shadow-lg transition-all ${
          toast.type === 'success'
            ? 'bg-emerald-950 border border-emerald-800 text-emerald-300'
            : 'bg-red-950 border border-red-800 text-red-300'
        }`}>
          {toast.type === 'success'
            ? <CheckCircle size={15} className="shrink-0" />
            : <XCircle size={15} className="shrink-0" />}
          {toast.message}
        </div>
      )}

      {/* Page header */}
      <div>
        <h1 className="text-xl font-semibold text-white">Settings</h1>
        <p className="mt-1 text-sm text-neutral-500">Configure your AI provider and notification preferences.</p>
      </div>

      {/* AI Provider section */}
      <section className="rounded-xl border border-neutral-800 bg-neutral-900/50 overflow-hidden">
        <div className="flex items-center gap-2.5 px-5 py-4 border-b border-neutral-800">
          <Key size={15} className="text-blue-400" />
          <h2 className="text-sm font-semibold text-white">AI Provider</h2>
        </div>
        <div className="px-5 py-5 space-y-4">
          {/* Provider dropdown */}
          <div>
            <label className="block text-xs font-medium text-neutral-400 mb-1.5">Provider</label>
            <select
              value={aiProvider}
              onChange={e => setAiProvider(e.target.value)}
              className="w-full bg-neutral-800 border border-neutral-700 text-neutral-200 text-sm rounded-lg px-3 py-2 focus:outline-none focus:ring-1 focus:ring-blue-500"
            >
              {AI_PROVIDERS.map(p => (
                <option key={p.value} value={p.value}>{p.label}</option>
              ))}
            </select>
          </div>

          {/* API Key */}
          <div>
            <label className="block text-xs font-medium text-neutral-400 mb-1.5">API Key</label>
            <div className="relative">
              <input
                type={showApiKey ? 'text' : 'password'}
                value={aiApiKey}
                onChange={e => setAiApiKey(e.target.value)}
                placeholder="Leave blank to keep existing key"
                className="w-full bg-neutral-800 border border-neutral-700 text-neutral-200 text-sm rounded-lg px-3 py-2 pr-10 focus:outline-none focus:ring-1 focus:ring-blue-500 placeholder-neutral-600"
              />
              <button
                type="button"
                onClick={() => setShowApiKey(v => !v)}
                className="absolute right-2.5 top-1/2 -translate-y-1/2 text-neutral-500 hover:text-neutral-300 transition-colors"
              >
                {showApiKey ? <EyeOff size={14} /> : <Eye size={14} />}
              </button>
            </div>
            <p className="mt-1 text-[11px] text-neutral-600">API keys are stored encrypted and never exposed.</p>
          </div>

          {/* Model */}
          <div>
            <label className="block text-xs font-medium text-neutral-400 mb-1.5">Model</label>
            <input
              type="text"
              value={aiModel}
              onChange={e => setAiModel(e.target.value)}
              placeholder={selectedProvider?.placeholder ?? 'Model name'}
              className="w-full bg-neutral-800 border border-neutral-700 text-neutral-200 text-sm rounded-lg px-3 py-2 focus:outline-none focus:ring-1 focus:ring-blue-500 placeholder-neutral-600"
            />
          </div>

          {/* Test connection */}
          <div className="pt-1">
            <button
              onClick={handleTestConnection}
              disabled={testing}
              className="flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium bg-neutral-800 border border-neutral-700 text-neutral-300 hover:bg-neutral-700 hover:text-white disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              {testing && <Loader2 size={13} className="animate-spin" />}
              Test Connection
            </button>
          </div>
        </div>
      </section>

      {/* Notifications section */}
      <section className="rounded-xl border border-neutral-800 bg-neutral-900/50 overflow-hidden">
        <div className="flex items-center gap-2.5 px-5 py-4 border-b border-neutral-800">
          <Bell size={15} className="text-blue-400" />
          <h2 className="text-sm font-semibold text-white">Notifications</h2>
        </div>
        <div className="px-5 py-5 space-y-4">
          {/* Email */}
          <div>
            <label className="block text-xs font-medium text-neutral-400 mb-1.5">Notification Email</label>
            <input
              type="email"
              value={notificationEmail}
              onChange={e => setNotificationEmail(e.target.value)}
              placeholder="you@example.com"
              className="w-full bg-neutral-800 border border-neutral-700 text-neutral-200 text-sm rounded-lg px-3 py-2 focus:outline-none focus:ring-1 focus:ring-blue-500 placeholder-neutral-600"
            />
          </div>

          {/* Weekly report toggle */}
          <div className="flex items-center justify-between py-1">
            <div>
              <p className="text-sm font-medium text-neutral-300">Weekly Report</p>
              <p className="text-xs text-neutral-600 mt-0.5">Receive a weekly summary of tech debt across your repositories.</p>
            </div>
            <button
              type="button"
              role="switch"
              aria-checked={weeklyReportEnabled}
              onClick={() => setWeeklyReportEnabled(v => !v)}
              className={`relative inline-flex h-5 w-9 shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors focus:outline-none ${
                weeklyReportEnabled ? 'bg-blue-600' : 'bg-neutral-700'
              }`}
            >
              <span
                className={`pointer-events-none inline-block h-4 w-4 rounded-full bg-white shadow transform transition-transform ${
                  weeklyReportEnabled ? 'translate-x-4' : 'translate-x-0'
                }`}
              />
            </button>
          </div>
        </div>
      </section>

      {/* Save button */}
      <div className="flex justify-end pb-4">
        <button
          onClick={handleSave}
          disabled={saving}
          className="flex items-center gap-2 px-5 py-2.5 rounded-lg text-sm font-semibold bg-blue-600 hover:bg-blue-500 text-white disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        >
          {saving && <Loader2 size={13} className="animate-spin" />}
          {saving ? 'Saving…' : 'Save Settings'}
        </button>
      </div>
    </div>
  );
}
