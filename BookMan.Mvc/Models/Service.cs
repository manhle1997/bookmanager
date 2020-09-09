using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace BookMan.Mvc.Models
{
    public class Service
    {
        private readonly string _dataFile = @"Data\data.xml";
        private readonly XmlSerializer _serializer = new XmlSerializer(typeof(HashSet<Book>));
        public HashSet<Book> Books { get; set; }

        public Service()
        {
            if (!File.Exists(_dataFile))
            {
                Books = new HashSet<Book>()
                {
                    new Book{Id = 1, Name = "A", Authors = "A", Publisher = "A", Year = 2020},
                    new Book{Id = 2, Name = "B", Authors = "B", Publisher = "B", Year = 2020},
                    new Book{Id = 3, Name = "C", Authors = "C", Publisher = "C", Year = 2020},
                    new Book{Id = 4, Name = "D", Authors = "D", Publisher = "D", Year = 2020},
                };
            }
            else
            {
                using var stream = File.OpenRead(_dataFile);
                Books = _serializer.Deserialize(stream) as HashSet<Book>;
            }
        }

        public Book[] Get()
        {
            return Books.ToArray();
        }
        public Book[] Get(string search)
        {
            var s = search.ToLower();
            return Books.Where(b =>
            b.Name.ToLower().Contains(s) ||
            b.Authors.ToLower().Contains(s) ||
            b.Publisher.ToLower().Contains(s) ||
            //b.Description.Contains(s) ||
            b.Year.ToString() == s
            ).ToArray();
        }

        public Book Get(int id)
        {
            return Books.FirstOrDefault(b => b.Id == id);//Trả về phần tử đầu tiên theo của HashSet Books theo điều kiện Id = id được truyền vào
        }

        public bool Add(Book book)
        {
            return Books.Add(book);//Gọi phương thức Add của HashSet để thêm book mới
        }

        public Book Create()
        {
            var max = Books.Max(b => b.Id);//gán max bằng Id lớn nhất
            var b = new Book() //Khởi tạo đối tượng mới với Id = max +1 và năm = năm hiện tại
            {
                Id = max + 1,
                Year = DateTime.Now.Year
            };
            return b; // Trả về đối tượng b;
        }

        public bool Update(Book book)
        {
            var b = Get(book.Id);
            return b != null && Books.Remove(b) && Books.Add(book);
        }

        public bool Dalete(int id)
        {
            var b = Get(id);
            return b != null && Books.Remove(b);
        }

        public void SaveChanges()
        {
            using var stream = File.Create(_dataFile);
            _serializer.Serialize(stream, Books);
        }
        public string GetDataPath(string file)
        {
            return $"Data\\{file}"; //hàm này chỉ trả về một dạng string là đường dẫn của File sách
        }
        public void Upload(Book book, IFormFile file)
        {
            if(file != null) //Nếu file dc tạo rồi
            {
                var path = GetDataPath(file.FileName);
                using var stream = new FileStream(path, FileMode.Create);
                file.CopyTo(stream);
                book.DataFile = file.FileName;
            }
        }
        public (Stream, string) Download (Book book)
        {
            var memory = new MemoryStream();
            using var stream = new FileStream(GetDataPath(book.DataFile), FileMode.Open);
            stream.CopyTo(memory);
            memory.Position = 0;
            var type = Path.GetExtension(book.DataFile) switch
            {
                "pdf" => "application/pdf",
                "docx" => "application/vnd.ms-word",
                "doc" => "application/vnd.ms-word",
                "txt" => "text/plain",
                _ => "application/pdf"
            };
            return (memory, type);
        }
        public (Book[] books, int pages, int page) Paging(int page)
        {
            int size = 3;
            int pages = (int)Math.Ceiling((double)Books.Count / size);
            var books = Books.Skip((page - 1) * size).Take(size).ToArray();
            return (books, pages, page);
        }
        
    }
}
