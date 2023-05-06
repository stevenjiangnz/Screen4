resource "azurerm_resource_group" "example" {
  name     = "WG-Screen4"
  location = "australiaeast"

  tags = {
    environment = "dev"
    owner       = "Steven Jiang"
  }
}