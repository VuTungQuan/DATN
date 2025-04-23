using System;
using System.Collections.Generic;

namespace DATN.DTO
{
    // DTO cho response API đơn giản
    public class ResponseDTO<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        // Tạo response thành công
        public static ResponseDTO<T> SuccessResult(T data, string message = "Thao tác thành công")
        {
            return new ResponseDTO<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        // Tạo response lỗi
        public static ResponseDTO<T> ErrorResult(string message, List<string>? errors = null)
        {
            return new ResponseDTO<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }

        // Tạo response lỗi từ exception
        public static ResponseDTO<T> ExceptionResult(Exception ex)
        {
            return new ResponseDTO<T>
            {
                Success = false,
                Message = "Đã xảy ra lỗi khi xử lý yêu cầu",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    // DTO cho response API có phân trang
    public class PaginatedResponseDTO<T>
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = "Thao tác thành công";
        public List<T> Items { get; set; } = new List<T>();
        public int TotalItems { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        // Tạo response có phân trang
        public static PaginatedResponseDTO<T> Create(List<T> items, int totalItems, int pageNumber, int pageSize)
        {
            return new PaginatedResponseDTO<T>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }
    }
} 