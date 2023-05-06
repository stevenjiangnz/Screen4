resource "azurerm_resource_group" "screen_rg" {
  name     = "WG-Screen4"
  location = "australiaeast"

  tags = var.tags
}


resource "azurerm_storage_account" "screen_sa" {
  name                     = "sascreen4sj"
  resource_group_name      = azurerm_resource_group.screen_rg.name
  location                 = "australiaeast"
  account_tier             = "Standard"
  account_replication_type = "LRS"

  tags = var.tags
}