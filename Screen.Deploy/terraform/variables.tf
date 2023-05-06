variable "tags" {
  type = map(string)
  default = {
    environment = "dev"
    owner       = "steven.jiang"
  }
}