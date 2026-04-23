export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken?: string;
}

export interface JwtPayload {
  sub: string;
  unique_name: string;
  exp: number;
  token_version?: number;
}
