using System.Security.Cryptography;
using System.Text.Json;
using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class SeedData
    {
        public static async Task SeedUsers(UserManager<AppUser> userManager,RoleManager<AppRole>roleManager){
            if(await userManager.Users.AnyAsync()) return;
            var userData=await System.IO.File.ReadAllTextAsync("Data/UserSeedData.json");
            var users=JsonSerializer.Deserialize<List<AppUser>>(userData);
            //if(users==null) return;

            //addin roles
            //var roles = Enum.GetNames(typeof(Roles)).Length;
            foreach(var role in Enum.GetNames(typeof(Roles))){
                await roleManager.CreateAsync(new AppRole{Name=role.ToString()});
            }
            //adding users
            foreach(var user in users){
                user.UserName=user.UserName.ToLower();
                await userManager.CreateAsync(user, "@Ams123");
                await userManager.AddToRoleAsync(user,Roles.Member.ToString());
            }
            //adding admin
            var admin=new AppUser{
                UserName="admin"
            };
            await userManager.CreateAsync(admin,"@Ams123");
            await userManager.AddToRolesAsync(admin,new[]{Roles.Admin.ToString(),Roles.Moderator.ToString()});
        }
    }
}