provider "azurerm" {
  features {}
}

terraform {
  backend "azurerm" {
    resource_group_name  = "WG-SJ-General"
    storage_account_name = "sascreen4terraform"
    container_name       = "terraform-state"
    key                  = "screen/terraform.tfstate"
  }
}
