namespace Project_385.Models
{
    public class RoleModels
    {
        public string Id { get; set; }
        public string RoleName { get; set; }
        public int CanManageProduct { get; set; }
        public int CanManageOrder { get; set; }    
        public int CanManageUser { get; set; }     
        public int CanManageBrand { get; set; }    
        public int CanManageCategory { get; set; } 
        public int Status { get; set; }            
    }
}