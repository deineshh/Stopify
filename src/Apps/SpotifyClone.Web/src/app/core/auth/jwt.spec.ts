import { jwtExpiry } from './jwt';

const encode = (value: object) =>
  btoa(JSON.stringify(value)).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');

describe('jwtExpiry', () => {
  it('reads the exp claim', () => {
    const token = `${encode({ alg: 'HS256' })}.${encode({ sub: 'u1', exp: 1_800_000_000 })}.signature`;
    expect(jwtExpiry(token)?.toISOString()).toBe(new Date(1_800_000_000_000).toISOString());
  });

  it('returns null for malformed tokens', () => {
    expect(jwtExpiry('not-a-jwt')).toBeNull();
    expect(jwtExpiry('a.%%%.c')).toBeNull();
    expect(jwtExpiry(`${encode({})}.${encode({ sub: 'u1' })}.x`)).toBeNull();
  });
});
