﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SportFieldBooking.Helper.Exceptions;

namespace SportFieldBooking.Helper.Pagination
{
    public static class PagingExtensions
    {
        /// <summary>
        /// Auth: Hung
        /// Created: 20/04/2022
        /// LINQ extension method cho viec phan trang
        /// </summary>
        /// <typeparam name="TSource"> Generic datatype </typeparam>
        /// <param name="source"> Query object dau vao </param>
        /// <param name="pageIndex"> So thu tu trang </param>
        /// <param name="pageSize"> So instance trong mot trang </param>
        /// <param name="total"> Tong so instance trong database </param>
        /// <returns> Query object cua cac user trong mot trang cu the </returns>
        /// <exception cref="InvalidPageException"> So trang nhap vao khong ton tai </exception>
        public static IQueryable<TSource> GetPagedResult<TSource>(this IQueryable<TSource> source, long pageIndex, int pageSize, long total)
        {
            if (((pageIndex - 1) * pageSize + pageSize) > total || pageIndex < 0){
                throw new InvalidPageException();
            } 
            var result = source.Skip((int)((pageIndex - 1) * pageSize)).Take(pageSize);
            return result;
        }
    }
}
