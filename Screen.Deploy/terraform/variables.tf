variable "tags" {
  type = map(string)
  default = {
    environment = "dev"
    owner       = "steven.jiang"
    purpose     = "Screen"
  }
}

variable "localtion" {
  type = string
  default = "australiaeast"
}