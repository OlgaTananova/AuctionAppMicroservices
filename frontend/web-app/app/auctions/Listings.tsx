'use client'
import React, { useEffect, useState } from 'react'
import AuctionCard from './AuctionCard';
import { Auction, PagedResult } from '../../types/index'
import AppPagination from '../components/AppPagination';
import { getData } from '../actions/auctionAction';
import Filters from './Filters';
import { useParams } from 'next/navigation';
import { useParamsStore } from '@/hooks/useParamsStore';
import { shallow } from 'zustand/shallow';
import qs from 'query-string';
import EmptyFilter from '../components/EmptyFilter';
import { useAuctionStore } from '@/hooks/useAuctionStore';


export default function Listings() {
  const params = useParamsStore(state => ({
    pageNumber: state.pageNumber,
    pageSize: state.pageSize,
    searchTerm: state.searchTerm,
    orderBy: state.orderBy,
    filterBy: state.filterBy
  }), shallow);

  const { auctions, totalCount, pageCount } = useAuctionStore(state => ({
    auctions: state.auctions,
    totalCount: state.totalCount,
    pageCount: state.pageCount
  }), shallow);

  const setData = useAuctionStore(state => state.setData);
  const resetAuctions = useAuctionStore(state => state.resetAuctions);

  const setParams = useParamsStore(state => state.setParams);
  //const url = qs.stringifyUrl({ url: '', query: params })

  function setPageNumber(pageNumber: number) {
    setParams({ pageNumber })
  }

  useEffect(() => {
    const url = qs.stringifyUrl({ url: '', query: params })
    console.log(url);
    getData(url).then(data => {
      setData(data);
    })
  }, [params, setData]);

  if (!auctions) return <h3>Loading...</h3>

  return (
    <>
      <Filters />
      {totalCount === 0 ? (
        <EmptyFilter showReset />
      ) : (
        <>
          <div className='grid grid-cols-4 gap-6'>
            {auctions.map((auction) => (
              <AuctionCard auction={auction} key={auction.id} />
            ))}
          </div>
          <div className='flex justify-center mt-4'>
            <AppPagination currentPage={params.pageNumber} pageCount={pageCount} pageChanged={setPageNumber} />
          </div>
        </>
      )}
    </>
  )
}
