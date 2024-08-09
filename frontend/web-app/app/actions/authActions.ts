import { getServerSession } from "next-auth";
import { getToken } from "next-auth/jwt";
import { cookies, headers } from "next/headers";
import { NextApiRequest } from "next";
import authOptions from "../api/auth/[...nextauth]/options";


// Function to get the server-side session using NextAuth options
export async function getSession() {
    return await getServerSession(authOptions) // Fetch the server-side session
}

// Function to get the current user from the session
export async function getCurrentUser() {
    try {
        const session = await getSession(); // Get the session
        if (!session) return null; // Return null if no session is found
        return session.user; // Return the user from the session
    } catch (error) {
        return null; // Return null in case of an error
    }
}

// Function to get the JWT token, working around limitations by constructing a request object

export async function getTokenWorkarount(){
    // Construct a mock NextApiRequest object with headers and cookies
    const req = {
        headers: Object.fromEntries(headers() as Headers), // Convert headers to an object
        cookies: Object.fromEntries(cookies().getAll().map(c=> [c.name, c.value])) // Convert cookies to an object

    } as NextApiRequest
    return await getToken({req}) // Get the token using the constructed request object
}