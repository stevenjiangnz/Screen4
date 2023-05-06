resource "azurerm_resource_group" "screen_rg" {
  name     = "WG-Screen4"
  location = var.localtion

  tags = var.tags
}

resource "azurerm_storage_account" "screen_sa" {
  name                     = "sascreen4sj"
  resource_group_name      = azurerm_resource_group.screen_rg.name
  location                 = var.localtion
  account_tier             = "Standard"
  account_replication_type = "LRS"

  tags = var.tags
}


resource "azurerm_storage_container" "screen_container" {
  name                  = "screencontainer"
  storage_account_name  = azurerm_storage_account.screen_sa.name
  container_access_type = "private"
}