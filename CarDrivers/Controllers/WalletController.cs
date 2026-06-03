// ChargingStationManagement.API/Controllers/WalletController.cs
using ChargingStationManagement.Domain.Enums;
using ChargingStationManagement.Services.DTOs;
using ChargingStationManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static ChargingStationManagement.API.Controllers.StationsController;

namespace ChargingStationManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly ILogger<WalletController> _logger;

        public WalletController(
            IWalletService walletService,
            ILogger<WalletController> logger)
        {
            _walletService = walletService;
            _logger = logger;
        }

        /// <summary>
        /// 获取钱包信息
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(WalletDto), 200)]
        public async Task<IActionResult> GetWallet()
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var wallet = await _walletService.GetWalletByUserIdAsync(userId);

                if (wallet == null)
                    return NotFound(new ApiResponse<WalletDto>
                    {
                        Success = false,
                        Message = "Wallet not found"
                    });

                return Ok(new ApiResponse<WalletDto>
                {
                    Success = true,
                    Data = wallet
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wallet");
                return BadRequest(new ApiResponse<WalletDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 钱包充值
        /// </summary>
        [HttpPost("recharge")]
        [ProducesResponseType(typeof(TransactionDto), 200)]
        public async Task<IActionResult> Recharge([FromBody] RechargeRequest request)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var transaction = await _walletService.RechargeWalletAsync(
                    userId,
                    request.Amount,
                    request.PaymentMethod.ToString());

                return Ok(new ApiResponse<TransactionDto>
                {
                    Success = true,
                    Data = transaction,
                    Message = "Recharge successful"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recharging wallet");
                return BadRequest(new ApiResponse<TransactionDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 获取钱包交易记录
        /// </summary>
        [HttpGet("transactions")]
        [ProducesResponseType(typeof(List<TransactionDto>), 200)]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var transactions = await _walletService.GetWalletTransactionsAsync(
                    userId, startDate, endDate);

                // 分页
                var totalCount = transactions.Count;
                var pagedTransactions = transactions
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Ok(new ApiPagedResponse<List<TransactionDto>>
                {
                    Success = true,
                    Data = pagedTransactions,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wallet transactions");
                return BadRequest(new ApiResponse<List<TransactionDto>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 检查余额是否充足
        /// </summary>
        [HttpPost("check-balance")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<IActionResult> CheckBalance([FromBody] CheckBalanceRequest request)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var hasBalance = await _walletService.CheckBalanceAsync(userId, request.Amount);

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = hasBalance,
                    Message = hasBalance ? "Balance is sufficient" : "Insufficient balance"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking balance");
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 管理员充值（仅管理员可用）
        /// </summary>
        [HttpPost("admin/recharge")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(TransactionDto), 200)]
        public async Task<IActionResult> AdminRecharge([FromBody] AdminRechargeRequest request)
        {
            try
            {
                var adminId = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;

                var transaction = await _walletService.AdminRechargeAsync(
                    request.UserId,
                    request.Amount,
                    adminId);

                return Ok(new ApiResponse<TransactionDto>
                {
                    Success = true,
                    Data = transaction,
                    Message = "Admin recharge successful"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in admin recharge");
                return BadRequest(new ApiResponse<TransactionDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // 请求DTO
        public class RechargeRequest
        {
            public decimal Amount { get; set; }
            public PaymentMethod PaymentMethod { get; set; }
        }

        public class CheckBalanceRequest
        {
            public decimal Amount { get; set; }
        }

        public class AdminRechargeRequest
        {
            public string UserId { get; set; }
            public decimal Amount { get; set; }
        }

        // 分页API响应
        public class ApiPagedResponse<T> : ApiResponse<T>
        {
            public int Page { get; set; }
            public int PageSize { get; set; }
            public int TotalCount { get; set; }
            public int TotalPages { get; set; }
        }
    }
}