'use client';

import { useAuth } from '@clerk/nextjs';

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000';

export function useApiClient() {
  const { getToken } = useAuth();

  const get = async <T>(path: string): Promise<T> => {
    const token = await getToken();
    const res = await fetch(`${API_URL}${path}`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    if (!res.ok) throw new Error(`API ${res.status}`);
    return res.json() as Promise<T>;
  };

  const post = async <TReq, TRes>(path: string, body?: TReq): Promise<TRes> => {
    const token = await getToken();
    const res = await fetch(`${API_URL}${path}`, {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: body !== undefined ? JSON.stringify(body) : undefined,
    });
    if (!res.ok) {
      const err = await res.json().catch(() => ({})) as Record<string, string>;
      throw new Error(err?.error ?? err?.title ?? `API ${res.status}`);
    }
    // 202 Accepted has no body
    if (res.status === 202) return undefined as unknown as TRes;
    return res.json() as Promise<TRes>;
  };

  return { get, post };
}
