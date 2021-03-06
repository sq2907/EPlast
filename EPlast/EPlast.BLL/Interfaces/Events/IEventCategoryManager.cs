﻿using EPlast.BLL.DTO.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EPlast.BLL.Interfaces.Events
{
    /// <summary>
    ///  Implements  operations for work with event categories.
    /// </summary>
    public interface IEventCategoryManager
    {
        Task<IEnumerable<EventCategoryDTO>> GetDTOAsync();

        /// <summary>
        /// Get list of event categories by event type Id.
        /// </summary>
        /// <returns>List of event categories of the appropriate event type.</returns>
        /// <param name="eventTypeId">The Id of event type</param>
        Task<IEnumerable<EventCategoryDTO>> GetDTOByEventTypeIdAsync(int eventTypeId);

        /// <summary>
        /// Get list of event categories by event type Id.
        /// </summary>
        /// <returns>List of event categories of the appropriate event type.</returns>
        /// <param name="eventTypeId">The Id of event type</param>
        /// <param name="page">A number of the page</param>
        /// <param name="pageSize">A count of categories to display</param>
        Task<IEnumerable<EventCategoryDTO>> GetDTOByEventPageAsync(int eventTypeId, int page, int pageSize, string CategoryName = null);
    }
}
