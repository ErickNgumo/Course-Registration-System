import { createContext, useContext, useState, useCallback, useEffect, type ReactNode } from "react";
import { jwtDecode, isTokenExpired } from "@/lib/jwt";
import type { JwtClaims } from "@/types/api";

interface AuthState {
  userId: string | null;
  role: "Student" | "Administrator" | null;
  userName: string | null;
}

interface AuthContextType extends AuthState {
  isAuthenticated: boolean;
  isStudent: boolean;
  isAdmin: boolean;
  login: (token: string, role: "Student" | "Administrator", userId: string, userName: string) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>(() => {
    const token = localStorage.getItem("accessToken");
    const role = localStorage.getItem("userRole");
    const userId = localStorage.getItem("userId");
    const userName = localStorage.getItem("userName");

    if (token && role && userId && !isTokenExpired(token)) {
      return {
        userId,
        role: role as "Student" | "Administrator",
        userName,
      };
    }

    localStorage.removeItem("accessToken");
    localStorage.removeItem("userRole");
    localStorage.removeItem("userId");
    localStorage.removeItem("userName");
    return { userId: null, role: null, userName: null };
  });

  useEffect(() => {
    const checkToken = () => {
      const token = localStorage.getItem("accessToken");
      if (token && isTokenExpired(token)) {
        setState({ userId: null, role: null, userName: null });
        localStorage.removeItem("accessToken");
        localStorage.removeItem("userRole");
        localStorage.removeItem("userId");
        localStorage.removeItem("userName");
      }
    };

    const id = setInterval(checkToken, 60_000);
    return () => clearInterval(id);
  }, []);

  const login = useCallback(
    (token: string, role: "Student" | "Administrator", userId: string, userName: string) => {
      localStorage.setItem("accessToken", token);
      localStorage.setItem("userRole", role);
      localStorage.setItem("userId", userId);
      localStorage.setItem("userName", userName);
      setState({ userId, role, userName });
    },
    [],
  );

  const logout = useCallback(() => {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("userRole");
    localStorage.removeItem("userId");
    localStorage.removeItem("userName");
    setState({ userId: null, role: null, userName: null });
  }, []);

  return (
    <AuthContext.Provider
      value={{
        ...state,
        isAuthenticated: state.userId !== null,
        isStudent: state.role === "Student",
        isAdmin: state.role === "Administrator",
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export { AuthContext };