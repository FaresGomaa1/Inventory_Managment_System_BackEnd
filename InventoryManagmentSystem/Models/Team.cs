using InventoryManagmentSystem.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class Team : BaseClass
{
    public ICollection<User> Members { get; set; }
    public ICollection<Request> Requests { get; set; }
}
