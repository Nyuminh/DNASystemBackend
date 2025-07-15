using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DNASystemBackend.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly DnasystemContext _context;

        public NotificationService(INotificationRepository repository, DnasystemContext context)
        {
            _repository = repository;
            _context = context;
        }

        public async Task<Notification> GetNotificationAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Notification ID không được để trống.", nameof(id));

            var notification = await _repository.GetByIdAsync(id);
            if (notification == null)
                throw new KeyNotFoundException($"Notification ID {id} không tồn tại.");

            return notification;
        }

        public async Task<IEnumerable<Notification>> GetNotificationsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID không được để trống.", nameof(userId));

            return await _repository.GetAllAsync(userId);
        }

        public async Task<Notification> CreateNotificationAsync(Notification notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            if (string.IsNullOrEmpty(notification.NotificationId))
                notification.NotificationId = await GenerateNotificationId(); 

            notification.IsRead = false;

            await _repository.AddAsync(notification);
            return notification;
        }

        public async Task UpdateNotificationAsync(Notification notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            if (string.IsNullOrEmpty(notification.NotificationId))
                throw new ArgumentException("Notification ID không được để trống.", nameof(notification.NotificationId));

            await _repository.UpdateAsync(notification);
        }

        public async Task DeleteNotificationAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Notification ID không được để trống.", nameof(id));

            await _repository.DeleteAsync(id);
        }

        private async Task<string> GenerateNotificationId()
        {
            var existingIds = await _context.Notifications
                .Select(n => n.NotificationId)
                .Where(id => id.StartsWith("notif-") && id.Length == 8)
                .ToListAsync();

        int counter = 1;
        string newId;
        do
        {
            newId = $"U{counter:D03}";
            counter++;
        } while (existingIds.Contains(newId) && counter < 1000);

        if (counter >= 1000)
        {
            newId = $"U{DateTime.Now.Ticks % 1000000:D06}";
        }

        return newId;
        }
    }
}