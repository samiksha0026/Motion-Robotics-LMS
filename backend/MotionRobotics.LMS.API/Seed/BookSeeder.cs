using MotionRobotics.LMS.API.Data;
using MotionRobotics.LMS.API.Models;
using System.Threading.Tasks;
using System.Linq;

namespace MotionRobotics.LMS.API.Seed
{
    public static class BookSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!context.Books.Any())
            {
                var books = new List<Book>
                {
                    new Book { Title = "Mech-Tech", Author = "Samiksha Thorat", Description = "Mechanics Kits" },
                    new Book { Title = "Basic of Electronics", Author = "Swara Thorat", Description = "How Machines and Robots works with basic Fundamentals of Science." },
                    new Book { Title = "Electro-Mechanical", Author = "Swaranjali", Description = "Control the bots with the help of wireless remote." },
                    new Book { Title = "Digi-Coding", Author = "Swaranjali", Description = "Introduction to basic coding and microcontroller." },
                    new Book { Title = "Digi-Sense", Author = "Swara", Description = "Integrating sensors with micro controller" },
                    new Book { Title = "Wireless & IOT", Author = "Swaranjali", Description = "Control the bots with the help of wireless remote." },
                };

                context.Books.AddRange(books);
                await context.SaveChangesAsync();
            }
        }
    }
}
