using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Catalog.API.Infrastructure.Exceptions;
using Pgvector;

namespace Catalog.API.Model
{
    public class CatalogItem
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string PictureFileName { get; set; }
        public int CatalogTypeId { get; set; }
        public CatalogType CatalogType { get; set; }
        public int CatalogBrandId { get; set; }
        public CatalogBrand CatalogBrand { get; set; }
        public int AvaliableStock { get; set; }
        public int RestockThreshold { get; set; }
        public int MaxStockThreshold { get; set; }
        [JsonIgnore]
        public Vector Embedding { get; set; }
        public bool OnReorder { get; set; }

        public CatalogItem(){}

        /// <summary>
        /// Decrements the quantity of a particular item in inventory and ensures the restockThreshold hasn't
        /// been breached. If so, a RestockRequest is generated in CheckThreshold. 
        /// 
        /// If there is sufficient stock of an item, then the integer returned at the end of this call should be the same as quantityDesired. 
        /// In the event that there is not sufficient stock available, the method will remove whatever stock is available and return that quantity to the client.
        /// In this case, it is the responsibility of the client to determine if the amount that is returned is the same as quantityDesired.
        /// It is invalid to pass in a negative number. 
        /// </summary>
        /// <param name="quantityDesired"></param>
        /// <returns>int: Returns the number actually removed from stock. </returns>
        /// 

        public int RemoveStock(int quantityDesired)
        {
            if(AvaliableStock == 0)
            {
                throw new CatalogDomainException($"Empty stock, product item {Name} is sold out");
            }

            if(quantityDesired <= 0)
            {
                throw new CatalogDomainException($"Item units desired should be greater than zero");
            }

            int removed = Math.Min(quantityDesired, this.AvaliableStock);

            this.AvaliableStock -= removed;

            return removed;
        }

        /// <summary>
        /// Increments the quantity of a particular item in inventory.
        /// <param name="quantity"></param>
        /// <returns>int: Returns the quantity that has been added to stock</returns>
        /// </summary>
        /// 
        public int AddStock(int quantity)
        {
            int original = this.AvaliableStock;

            // The quantity that the client is trying to add to stock is greater than what can be physically accommodated in the Warehouse
            if((this.AvaliableStock + quantity) > this.MaxStockThreshold)
            {
                this.AvaliableStock += (this.MaxStockThreshold - this.AvaliableStock);
            }
            else
            {
                this.AvaliableStock += quantity;
            }

            this.OnReorder = false;

            return this.AvaliableStock - original;
        }
    }
}
