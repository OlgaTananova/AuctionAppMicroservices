import NextAuth, { NextAuthOptions } from "next-auth";
import DuendeIdentityServer6 from "next-auth/providers/duende-identity-server6";

// Define authentication options for NextAuth
export const authOptions: NextAuthOptions = {
    session: {
        strategy: "jwt"  // Use JWT strategy for session handling
    },
    // Configure Duende Identity Server 6 as an authentication provider
    providers: [
        DuendeIdentityServer6({
            id: 'id-server',  // Unique identifier for the provider
            clientId: 'nextApp',  // Client ID for the application
            clientSecret: 'secret',  // Client secret for the application
            issuer: 'http://localhost:5000',  // URL of the Identity Server
            authorization: { params: { scope: 'openid profile auctionApp' } },  // Authorization parameters
            idToken: true  // Indicate that an ID token should be returned
        })
    ],
    callbacks: {
        // Define a callback to handle JWT tokens
        async jwt({ token, profile, account }) {
            // If profile data is available, add the username to the token
            if (profile) {
                token.username = profile.username
            }
            // If account data is available, add the access token to the token
            if (account) {
                token.access_token = account.access_token
            }
            return token; // Return the updated token
        },
        // Define a callback to handle session data
        async session({ session, token }) {
            // If token data is available, add the username to the session user
            if (token) {
                session.user.username = token.username
            }
            return session;  // Return the updated session
        }
    }
}
const handler = NextAuth(authOptions);
export { handler as GET, handler as POST }