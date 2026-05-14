using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prospera.Application.Features.Transactions.Commands;
using Prospera.Application.Features.Transactions.Queries;
using Prospera.Contracts.DTOs.Transaction;

namespace Prospera.API.Controllers;

/// <summary>
/// Endpoints for managing financial transactions
/// Supports both manual entry and automatic Stripe integration
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(IMediator mediator, ILogger<TransactionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Record a new transaction (manual entry)
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="request">Transaction details (amount, type, description)</param>
    /// <returns>The created transaction</returns>
    /// <response code="201">Transaction created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">User not found</response>
    [HttpPost("users/{userId}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddTransaction(Guid userId, AddTransactionRequest request)
    {
        _logger.LogInformation("Recording transaction for user: {UserId}, Type: {TransactionType}, Amount: {Amount}", 
            userId, request.Type, request.Amount);

        try
        {
            var command = new AddTransactionCommand 
            { 
                UserId = userId, 
                Amount = request.Amount, 
                Type = request.Type.ToString(),
                Description = request.Description ?? string.Empty
            };
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetTransactionById), new { userId, id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording transaction for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get transaction by ID
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="id">The transaction ID</param>
    /// <returns>Transaction details</returns>
    /// <response code="200">Transaction found</response>
    /// <response code="404">Transaction or user not found</response>
    [HttpGet("users/{userId}/transactions/{id}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransactionById(Guid userId, Guid id)
    {
        _logger.LogInformation("Fetching transaction {TransactionId} for user {UserId}", id, userId);

        try
        {
            var query = new GetTransactionQuery { UserId = userId, TransactionId = id };
            var result = await _mediator.Send(query);

            if (result == null)
            {
                _logger.LogWarning("Transaction {TransactionId} not found for user {UserId}", id, userId);
                return NotFound();
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching transaction {TransactionId}", id);
            throw;
        }
    }

    /// <summary>
    /// Get all transactions for a user with pagination
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="skip">Number of transactions to skip (pagination)</param>
    /// <param name="take">Number of transactions to take (pagination)</param>
    /// <returns>List of transactions</returns>
    /// <response code="200">Transactions retrieved</response>
    /// <response code="404">User not found</response>
    [HttpGet("users/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserTransactions(Guid userId, [FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        _logger.LogInformation("Fetching transactions for user: {UserId}, Skip: {Skip}, Take: {Take}", userId, skip, take);

        try
        {
            var query = new GetUserTransactionsQuery { UserId = userId, Skip = skip, Take = take };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching transactions for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Update transaction details
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="id">The transaction ID</param>
    /// <param name="request">Updated transaction information</param>
    /// <returns>Updated transaction</returns>
    /// <response code="200">Transaction updated</response>
    /// <response code="404">Transaction or user not found</response>
    [HttpPut("users/{userId}/transactions/{id}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTransaction(Guid userId, Guid id, AddTransactionRequest request)
    {
        _logger.LogInformation("Updating transaction {TransactionId} for user {UserId}", id, userId);

        try
        {
            var command = new UpdateTransactionCommand 
            { 
                UserId = userId, 
                TransactionId = id, 
                Amount = request.Amount,
                Type = request.Type.ToString(),
                Description = request.Description ?? string.Empty
            };
            var result = await _mediator.Send(command);

            if (result == null)
            {
                _logger.LogWarning("Transaction {TransactionId} not found for user {UserId}", id, userId);
                return NotFound();
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating transaction {TransactionId}", id);
            throw;
        }
    }

    /// <summary>
    /// Delete a transaction
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="id">The transaction ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Transaction deleted</response>
    /// <response code="404">Transaction or user not found</response>
    [HttpDelete("users/{userId}/transactions/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTransaction(Guid userId, Guid id)
    {
        _logger.LogInformation("Deleting transaction {TransactionId} for user {UserId}", id, userId);

        try
        {
            var command = new DeleteTransactionCommand { UserId = userId, TransactionId = id };
            var result = await _mediator.Send(command);

            if (!result)
            {
                _logger.LogWarning("Transaction {TransactionId} not found for user {UserId}", id, userId);
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting transaction {TransactionId}", id);
            throw;
        }
    }

    /// <summary>
    /// Sync transactions from Stripe (auto-detection)
    /// Fetches recent transactions from Stripe Connect and imports them
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="stripeAccountId">Optional Stripe Connect account ID</param>
    /// <returns>List of synced transactions</returns>
    /// <response code="200">Transactions synced successfully</response>
    /// <response code="401">Unauthorized - user not authenticated</response>
    /// <response code="404">User not found or no Stripe account connected</response>
    [HttpPost("users/{userId}/sync-stripe")]
    [ProducesResponseType(typeof(IEnumerable<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SyncStripeTransactions(Guid userId, [FromQuery] string? stripeAccountId = null)
    {
        _logger.LogInformation("Syncing Stripe transactions for user: {UserId}", userId);

        try
        {
            var command = new SyncStripeTransactionsCommand 
            { 
                UserId = userId, 
                StripeAccountId = stripeAccountId 
            };
            var result = await _mediator.Send(command);

            _logger.LogInformation("Successfully synced {Count} transactions from Stripe for user: {UserId}", 
                result.Count(), userId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing Stripe transactions for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get the Stripe Connect onboarding URL for a user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <returns>A URL to redirect the user to Stripe for authentication</returns>
    [HttpGet("users/{userId}/stripe-connect-url")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStripeConnectUrl(Guid userId)
    {
        _logger.LogInformation("Creating Mock Stripe Connect link for user: {UserId}", userId);
        
        // For testing/demo purposes, we return the deep link directly.
        // This simulates a successful redirect from Stripe.
        var mockRedirectUrl = "prospera://stripe-callback?code=mock_test_code&state=" + userId.ToString();
        
        return Ok(new { url = mockRedirectUrl });
    }
}
