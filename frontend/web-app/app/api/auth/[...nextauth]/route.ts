import NextAuth from "next-auth";
import { NextAuthOptions } from "next-auth";
import DuendeIdentityServer6 from "next-auth/providers/duende-identity-server6";
import authOptions from "./options";

// Define authentication options for NextAuth

const handler = NextAuth(authOptions);
export { handler as GET, handler as POST };