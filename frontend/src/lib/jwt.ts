export function jwtDecode<T = Record<string, unknown>>(token: string): T | null {
  try {
    const payload = token.split(".")[1];
    return JSON.parse(atob(payload));
  } catch {
    return null;
  }
}

export function isTokenExpired(token: string): boolean {
  const decoded = jwtDecode<{ exp: number }>(token);
  if (!decoded?.exp) return false;
  return Date.now() >= decoded.exp * 1000;
}