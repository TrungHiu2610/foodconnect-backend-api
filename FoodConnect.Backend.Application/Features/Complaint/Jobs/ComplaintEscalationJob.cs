using FoodConnect.Backend.Application.Features.Complaint.Services;
using FoodConnect.Backend.Application.Interfaces;
using FoodConnect.Backend.Application.Interfaces.IRepositories;
using FoodConnect.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FoodConnect.Backend.Application.Features.Complaint.Jobs
{
    public class ComplaintEscalationJob
    {
        private readonly IOrderComplaintRepository _complaintRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ComplaintNotificationService _notificationService;
        private readonly ILogger<ComplaintEscalationJob> _logger;

        public ComplaintEscalationJob(
            IOrderComplaintRepository complaintRepository,
            IUnitOfWork unitOfWork,
            ComplaintNotificationService notificationService,
            ILogger<ComplaintEscalationJob> logger)
        {
            _complaintRepository = complaintRepository;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task EscalateExpiredComplaintsAsync()
        {
            try
            {
                _logger.LogInformation("Starting complaint escalation job...");

                // Get complaints that are ready for auto-escalation (created more than 48 hours ago and still PendingSeller)
                var expiredComplaints = await _complaintRepository.GetComplaintsReadyForAutoEscalationAsync();

                if (expiredComplaints.Count == 0)
                {
                    _logger.LogInformation("No complaints to escalate.");
                    return;
                }

                _logger.LogInformation($"Found {expiredComplaints.Count} complaint(s) to escalate.");

                await using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    foreach (var complaint in expiredComplaints)
                    {
                        complaint.Status = OrderComplaintStatusEnum.PendingAdmin;
                        _complaintRepository.Update(complaint);

                        _logger.LogInformation(
                            $"Escalated complaint {complaint.Id} for order {complaint.OrderId} to admin (seller did not respond within 48 hours).");
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation($"Successfully escalated {expiredComplaints.Count} complaint(s).");

                    // Send notifications after successful transaction
                    foreach (var complaint in expiredComplaints)
                    {
                        try
                        {
                            // Reload complaint with full details for notification
                            var complaintWithDetails = await _complaintRepository.GetComplaintWithDetailsAsync(complaint.Id);
                            if (complaintWithDetails != null)
                            {
                                await _notificationService.NotifyComplaintEscalatedAsync(complaintWithDetails);
                            }
                        }
                        catch (Exception notifyEx)
                        {
                            _logger.LogError(notifyEx, $"Failed to send escalation notification for complaint {complaint.Id}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while escalating complaints in transaction.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in complaint escalation job.");
            }
        }
    }
}
