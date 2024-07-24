import { Auction, PagedResult } from "@/types"
import { create } from "zustand"
import { devtools, persist } from 'zustand/middleware';
import { createWithEqualityFn } from "zustand/traditional"

type State = {
    auctions: Auction[]
    totalCount: number
    pageCount: number
}

type Actions = {
    setData: (data: PagedResult<Auction>) => void
    setCurrentPrice: (auctionId: string, amount: number) => void
    resetAuctions: () => void
}

const initialState: State = {
    auctions: [],
    pageCount: 0,
    totalCount: 0
}

export const useAuctionStore = createWithEqualityFn<State & Actions>()(devtools(
    (set) => ({
    ...initialState,
    setData: (data: PagedResult<Auction>) => {
        set(() => ({
            auctions: data.result,
            totalCount: data.totalCount,
            pageCount: data.pageCount
        }))
    },

    resetAuctions: () => set(initialState),

    setCurrentPrice: (auctionId: string, amount: number) => {
        set((state) => ({
            auctions: state.auctions.map((auction) => auction.id === auctionId 
                ? {...auction, currentHighBid: amount} : auction)
        }))
    }
})));