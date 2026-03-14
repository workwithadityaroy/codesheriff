'use server';

import { postApi } from '@/lib/api';
import { revalidatePath } from 'next/cache';

export async function registerRepo(
  owner: string,
  name: string,
  installationId: number,
  platform: string = 'github',
  accessToken: string = '',
): Promise<string> {
  let gitId = 0;
  let fullName = `${owner}/${name}`;

  if (platform === 'github') {
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

    const ghRepo = await ghRes.json() as { id: number; full_name: string };
    gitId = ghRepo.id;
    fullName = ghRepo.full_name;
  } else if (platform === 'gitlab') {
    // Fetch GitLab project ID using the personal access token
    const encodedPath = encodeURIComponent(`${owner}/${name}`);
    const glRes = await fetch(`https://gitlab.com/api/v4/projects/${encodedPath}`, {
      headers: { Authorization: `Bearer ${accessToken}` },
      cache: 'no-store',
    });

    if (!glRes.ok) {
      if (glRes.status === 404) {
        throw new Error(`Project "${owner}/${name}" not found on GitLab. Check the namespace and name.`);
      }
      if (glRes.status === 401) {
        throw new Error('GitLab access token is invalid or expired.');
      }
      throw new Error(`GitLab API error ${glRes.status}. Try again.`);
    }

    const glProject = await glRes.json() as { id: number; path_with_namespace: string };
    gitId = glProject.id;
    fullName = glProject.path_with_namespace;
  }

  const id = await postApi<object, string>('/api/v1/repositories', {
    gitHubId: gitId,
    owner,
    name,
    fullName,
    installationId,
    gitProvider: platform,
    accessToken,
  });

  revalidatePath('/repositories');
  return id;
}
