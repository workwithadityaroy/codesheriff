'use server';

import { postApi } from '@/lib/api';
import { revalidatePath } from 'next/cache';

export async function registerRepo(
  owner: string,
  name: string,
  installationId: number,
): Promise<string> {
  // Fetch GitHub repo ID from the public API (no auth needed, runs server-side)
  const ghRes = await fetch(`https://api.github.com/repos/${owner}/${name}`, {
    headers: { Accept: 'application/vnd.github.v3+json' },
    cache: 'no-store',
  });

  if (!ghRes.ok) {
    if (ghRes.status === 404) {
      throw new Error(`Repository "${owner}/${name}" not found on GitHub. Check the owner and name.`);
    }
    throw new Error(`GitHub API error ${ghRes.status}. Try again.`);
  }

  const ghRepo = await ghRes.json() as { id: number };

  const id = await postApi<object, string>('/api/v1/repositories', {
    gitHubId: ghRepo.id,
    owner,
    name,
    fullName: `${owner}/${name}`,
    installationId,
  });

  revalidatePath('/repositories');
  return id;
}
