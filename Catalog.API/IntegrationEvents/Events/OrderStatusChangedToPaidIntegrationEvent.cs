﻿using System.Collections.Generic;
using eShop.EventBus.Events;

namespace Catalog.API.IntegrationEvents.Events;

public record OrderStatusChangedToPaidIntegrationEvent(int OrderId, IEnumerable<OrderStockItem> OrderStockItems):IntegrationEvent;

