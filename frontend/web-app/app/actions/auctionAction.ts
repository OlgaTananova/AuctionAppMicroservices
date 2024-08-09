'use server'
import { FieldValues } from "react-hook-form";
import { revalidatePath } from "next/cache";
import { PagedResult, Auction, Bid } from "@/types";
import { fetchWrapper } from "../lib/fetchWrapper.ts";

export async function getData(query: string): Promise<PagedResult<Auction>> {

    return await fetchWrapper.get(`search${query}`)
}

export async function updateAuctionTest() {
    const data = {
        milage: Math.floor(Math.random() * 100000) + 1
    }
    return await fetchWrapper.put('auctions/6a5011a1-fe1f-47df-9a32-b5346b289391', data);
}

export async function createAuction(data: FieldValues) {
    return await fetchWrapper.post('auctions', data);
}

export async function getDetailedViewData(id: string): Promise<Auction> {
    return await fetchWrapper.get(`auctions/${id}`);
}

export async function updateAuction(data: FieldValues, id: string) {
    const res = await fetchWrapper.put(`auctions/${id}`, data);
    revalidatePath(`/auctions/${id}`);
    return res;
}

export async function deleteAuction(id: string) {
    return await fetchWrapper.del(`auctions/${id}`);
}

export async function getBidsForAuction(id: string): Promise<Bid[]> {
    return await fetchWrapper.get(`bids/${id}`);
}

export async function placeBidForAuction(auctionId: string, amount: number) {
    return await fetchWrapper.post(`bids?auctionId=${auctionId}&amount=${amount}`, {})
}