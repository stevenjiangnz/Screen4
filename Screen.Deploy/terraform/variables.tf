variable "tags" {
  type = map(string)
  default = {
    environment = "dev"
    owner       = "steven.jiang"
  }
}

variable "localtion" {
  type = string
  default = "australiaeast"
}