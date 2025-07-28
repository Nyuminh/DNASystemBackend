using DNASystemBackend.DTOs;
using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Document = QuestPDF.Fluent.Document;
namespace DNASystemBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly ITestResultService _service;

        public ResultsController(ITestResultService service)
        {
            _service = service;

        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestResult>>> GetAll()
        {
            var results = await _service.GetAllAsync();
            return Ok(results);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TestResult>> GetById(string id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }
        [HttpGet("by-booking/{bookingId}")]
        public async Task<ActionResult<TestResult>> GetByBookingId(string bookingId)
        {
            var result = await _service.GetByBookingIdAsync(bookingId);
            if (result == null)
                return NotFound(new { message = $"Không tìm thấy kết quả xét nghiệm cho lịch hẹn {bookingId}" });

            return Ok(result);
        }


        [HttpPost]
        [Authorize(Roles = "Staff")]
        public async Task<ActionResult<TestResult>> Create([FromBody] TestResultCreateDto dto)
        {
            var id = await _service.GenerateIdAsync();
            var result = new TestResult
            {
                ResultId = id,
                CustomerId = dto.CustomerId,
                StaffId = dto.StaffId,
                ServiceId = dto.ServiceId,
                BookingId = dto.BookingId,
                Date = dto.Date,
                Description = dto.Description,
                Status = dto.Status
            };

            var created = await _service.CreateAsync(result);
            return CreatedAtAction(nameof(GetById), new { id = created.ResultId }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateTestResultDTO updatedResult)
        {
            var success = await _service.UpdateAsync(id, updatedResult);
            return success ? Ok(new { message = "Cập nhật kết quả thành công." }) : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _service.DeleteAsync(id);
            return deleted ? Ok(new { message = "Xóa kết quả thành công." }) : NotFound();
        }
        [HttpGet("{id}/pdf")]
        [Authorize]
        public async Task<IActionResult> ExportToPdf(string id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound("Không tìm thấy kết quả xét nghiệm.");

            var pdfBytes = ResultsController.GeneratePdf(result);
            return File(pdfBytes, "application/pdf", $"TestResult_{id}.pdf");
        }
        public class LocusEntry
        {
            public string Locus { get; set; } = "";
            public string FatherAllele { get; set; } = "";
            public string ChildAllele { get; set; } = "";
        }

        public static class LocusParser
        {
            public static List<LocusEntry> ParseDescriptionToLocus(string? description)
            {
                var result = new List<LocusEntry>();
                if (string.IsNullOrWhiteSpace(description)) return result;

                var lines = description.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines.Skip(1)) // bỏ dòng header "Locus B Ct"
                {
                    var parts = line.Split('\t', StringSplitOptions.None);
                    if (parts.Length >= 3)
                    {
                        result.Add(new LocusEntry
                        {
                            Locus = parts[0].Trim(),
                            FatherAllele = parts[1].Trim(),
                            ChildAllele = parts[2].Trim()
                        });
                    }
                }

                return result;
            }
        }
        public static byte[] GeneratePdf(TestResult result)
        {
            var locusList = LocusParser.ParseDescriptionToLocus(result.Description);

            // --- CÔNG THỨC MỚI CHO matchPercentage (chỉ mang tính giả định và minh họa) ---
            int matchingLociCount = 0;
            int nonMatchingLociCount = 0; // Để đếm số locus hoàn toàn không khớp
            int totalLoci = locusList.Count;

            foreach (var entry in locusList)
            {
                // Chia chuỗi alleles bằng ';' hoặc ' ' hoặc cả hai
                string[] fatherAlleles = entry.FatherAllele.Split(new char[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                          .Select(s => s.Trim()).ToArray();
                string[] childAlleles = entry.ChildAllele.Split(new char[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                        .Select(s => s.Trim()).ToArray();

                bool childHasFatherAllele = false;
                foreach (var cAllele in childAlleles)
                {
                    if (fatherAlleles.Contains(cAllele))
                    {
                        childHasFatherAllele = true;
                        break;
                    }
                }

                if (childHasFatherAllele)
                {
                    matchingLociCount++;
                }
                else
                {
                    nonMatchingLociCount++;
                }
            }

            double displayedMatchPercentage;
            string conclusionText;
            Color conclusionColor;

            // Logic giả định cho xác suất và kết luận
            // Rất quan trọng: Đây vẫn là logic ĐƠN GIẢN, KHÔNG CHÍNH XÁC KHOA HỌC như phòng thí nghiệm.
            // Bạn nên nhận kết luận và độ tin cậy thực tế từ nguồn dữ liệu (database) nếu có.
            if (nonMatchingLociCount >= 2) // Nếu có 2 locus trở lên không khớp, thường là loại trừ
            {
                displayedMatchPercentage = 0.0;
                conclusionText = "Kết luận: LOẠI TRỪ quan hệ cha con";
                conclusionColor = Colors.Red.Darken2;
            }
            else if (nonMatchingLociCount == 1 && totalLoci >= 15) // Cho phép 1 sai khác nhỏ do đột biến, nếu số locus đủ lớn
            {
                displayedMatchPercentage = 99.9; // Vẫn có thể rất cao nếu 1 locus do đột biến
                conclusionText = "Kết luận: KHẲNG ĐỊNH quan hệ cha con (có thể có đột biến tại một locus)";
                conclusionColor = Colors.Green.Darken2;
            }
            else if (matchingLociCount == totalLoci && totalLoci > 0) // Tất cả các locus đều khớp hoàn hảo
            {
                displayedMatchPercentage = 99.9999; // Tỷ lệ cực cao cho sự trùng khớp hoàn hảo
                conclusionText = "Kết luận: KHẲNG ĐỊNH quan hệ cha con";
                conclusionColor = Colors.Green.Darken2;
            }
            else // Các trường hợp còn lại, có thể chưa đủ bằng chứng hoặc không khớp hoàn hảo
            {
                // Tỷ lệ khớp đơn thuần của số locus
                displayedMatchPercentage = totalLoci > 0 ? (double)matchingLociCount / totalLoci * 100 : 0;
                conclusionText = "Kết luận: CHƯA XÁC ĐỊNH (cần thêm phân tích hoặc mẫu)";
                conclusionColor = Colors.Orange.Darken2;

                // Nếu tỉ lệ thấp, cũng có thể coi là loại trừ
                if (displayedMatchPercentage < 50 && totalLoci > 5) // Một ngưỡng thấp giả định
                {
                    conclusionText = "Kết luận: LOẠI TRỪ quan hệ cha con (tỷ lệ trùng khớp thấp)";
                    conclusionColor = Colors.Red.Darken2;
                }
            }
            // --- KẾT THÚC CÔNG THỨC MỚI ---
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    
                    // Company header - fixed structure to avoid reusing containers
                    page.Header().Element(header =>
                    {
                        // Use a single container structure
                        header.Column(column =>
                        {
                            // Company info in a row
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("DNA TESTING VN").FontSize(20).Bold();
                                    col.Item().Text("123 Đường Cầu Giấy,Quận Cầu Giấy, Hà Nội").FontSize(10);
                                    col.Item().Text("Phone: (123) 456-7890 | Email: contact@dnatesting.com").FontSize(10);
                                });
                            });
                            
                            // Border as a separate item
                            column.Item().PaddingTop(10).BorderBottom(1).BorderColor(Colors.Grey.Medium);
                        });
                    });
                    
                    // Content section
                    page.Content().Element(content =>
                    {
                        content.Column(column =>
                        {
                            // Title section
                            column.Item().PaddingTop(10).Column(col =>
                            {
                                col.Item().Text($"KẾT QUẢ XÉT NGHIỆM DNA").FontSize(18).Bold().FontColor(Colors.Blue.Medium);
                                col.Item().Text($"Mã kết quả: {result.ResultId}").FontSize(14);
                            });
                            
                            column.Item().PaddingTop(10);
                            
                            // Customer info section
                            column.Item().Column(col =>
                            {
                                col.Item().Text("THÔNG TIN KHÁCH HÀNG").FontSize(14).Bold();
                                col.Item().Grid(grid =>
                                {
                                    grid.Columns(2);
                                    grid.Item().Text("Ngày xét nghiệm:").Bold();
                                    grid.Item().Text(result.Date?.ToString("dd/MM/yyyy") ?? "N/A");
                                    grid.Item().Text("Nhân viên phụ trách:").Bold();
                                    grid.Item().Text(result.Staff?.Fullname ?? "N/A");
                                    grid.Item().Text("Tên (B):").Bold();
                                    grid.Item().Text(result.Customer?.Fullname ?? "N/A");
                                    grid.Item().Text("Tên (Ct):").Bold();
                                    grid.Item().Text(result.Booking?.Relatives.FirstOrDefault()?.Fullname ?? "N/A");
                                    
                                    grid.Item().Text("Loại dịch vụ:").Bold();
                                    grid.Item().Text(result.Service?.Name ?? "N/A");
                                });
                            });
                            
                            column.Item().PaddingTop(10);
                            
                            // Analysis result section
                            column.Item().Background(Colors.Grey.Lighten3).Padding(10).Column(col =>
                            {
                                col.Item().Text("KẾT QUẢ PHÂN TÍCH").FontSize(14).Bold();
                                col.Item().Text(displayedMatchPercentage > 99.9 ?
                                                $"Tỉ lệ trùng khớp:99.99 %" :
                                                $"Tỉ lệ trùng khớp:0.00 %").FontSize(12);
                                col.Item().Text(displayedMatchPercentage > 99.9 ? 
                                    "Kết luận: KHẲNG ĐỊNH quan hệ cha con" : 
                                    "Kết luận: LOẠI TRỪ quan hệ cha con").FontSize(12).Bold();
                            });
                            
                            column.Item().PaddingTop(10);
                            
                            // Locus data table section
                            column.Item().Column(col => 
                            {
                                col.Item().Text("BẢNG DỮ LIỆU CHI TIẾT").FontSize(14).Bold();
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2); // Locus
                                        columns.RelativeColumn(3); // B (cha)
                                        columns.RelativeColumn(3); // Ct (con)
                                    });

                                    // Header
                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Locus").Bold();
                                        header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text(" (B)").Bold();
                                        header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text(" (Ct)").Bold();
                                    });

                                    foreach (var entry in locusList)
                                    {
                                        var matched = entry.FatherAllele == entry.ChildAllele;
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(entry.Locus);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                            .Text(entry.FatherAllele).FontColor(matched ? Colors.Green.Medium : Colors.Black);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                            .Text(entry.ChildAllele).FontColor(matched ? Colors.Green.Medium : Colors.Black);
                                    }
                                });
                            });
                        });
                    });
                    
                    // Signature and certification
                    page.Footer().Element(footer =>
                    {
                        footer.Column(column =>
                        {
                            column.Item().PaddingTop(10).BorderTop(1).BorderColor(Colors.Grey.Medium);
                            column.Item().PaddingTop(10).Text("Xác nhận kết quả").FontSize(12).Bold();
                            column.Item().PaddingTop(20).Text($"Ngày: {DateTime.Now:dd/MM/yyyy}").FontSize(10);
                            column.Item().Text($"Nhân viên ký tên: {result.Staff?.Fullname ?? "__________________"}").FontSize(10);
                            column.Item().PaddingTop(5).Text("Kết quả này có giá trị pháp lý và đã được kiểm chứng bởi phòng thí nghiệm của chúng tôi.").FontSize(8).Italic();
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

    }
}
