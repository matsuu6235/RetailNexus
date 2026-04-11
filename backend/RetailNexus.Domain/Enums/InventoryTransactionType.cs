namespace RetailNexus.Domain.Enums;

public enum InventoryTransactionType
{
    PurchaseReceive = 1,
    ShipmentOut = 2,
    ShipmentIn = 3,
    Disposal = 4,
    Adjustment = 5,
    InitialStock = 6,
    Sale = 11,
    Return = 12,
}
