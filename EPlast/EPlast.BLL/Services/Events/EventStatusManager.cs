﻿using EPlast.BLL.Interfaces.Events;
using EPlast.DataAccess.Repositories;
using System.Threading.Tasks;

namespace EPlast.BLL.Services.Events
{
    public class EventStatusManager : IEventStatusManager
    {
        private readonly IRepositoryWrapper _repoWrapper;

        public EventStatusManager(IRepositoryWrapper repoWrapper)
        {
            _repoWrapper = repoWrapper;
        }

        /// <inheritdoc />
        public async Task<int> GetStatusIdAsync(string statusName)
        {
            var status = await _repoWrapper.EventStatus
                .GetFirstAsync(predicate: e => e.EventStatusName == statusName);

            return status.ID;
        }
    }
}
