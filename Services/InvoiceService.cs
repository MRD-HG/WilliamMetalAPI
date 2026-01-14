using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using WilliamMetalAPI.Data;
using WilliamMetalAPI.DTOs;

namespace WilliamMetalAPI.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly WilliamMetalContext _context;
        private readonly IWebHostEnvironment _env;

        public InvoiceService(WilliamMetalContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<List<InvoiceDto>> GetInvoicesAsync()
        {
            var sales = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.Items)
                    .ThenInclude(i => i.Variant)
                    .ThenInclude(v => v.Product)
                .OrderByDescending(s => s.CreatedAt)
                .Take(100)
                .ToListAsync();

            var company = await GetCompanySettingsAsync();

            return sales.Select(sale => BuildInvoiceDto(sale, company)).ToList();
        }

        public async Task<InvoiceDto?> GetInvoiceAsync(string saleId)
        {
            var sale = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.Items)
                    .ThenInclude(i => i.Variant)
                    .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(s => s.Id == saleId);

            if (sale == null)
            {
                return null;
            }

            var company = await GetCompanySettingsAsync();
            return BuildInvoiceDto(sale, company);
        }

        public async Task<(byte[] Pdf, string InvoiceNumber)?> GetInvoicePdfAsync(string saleId)
        {
            var invoice = await GetInvoiceAsync(saleId);
            if (invoice == null)
            {
                return null;
            }

            var logoBytes = TryLoadLogoBytes();
            var pdfBytes = BuildInvoicePdf(invoice, logoBytes);
            return (pdfBytes, invoice.InvoiceNumber);
        }

        private byte[]? TryLoadLogoBytes()
        {
            try
            {
                var webRoot = _env.WebRootPath;
                if (string.IsNullOrWhiteSpace(webRoot)) return null;

                var logoPath = Path.Combine(webRoot, "resources", "logo.png");
                if (!File.Exists(logoPath)) return null;
                return File.ReadAllBytes(logoPath);
            }
            catch
            {
                return null;
            }
        }

        private static InvoiceDto BuildInvoiceDto(Models.Sale sale, Models.CompanySettings? company)
        {
            var dueDate = sale.CreatedAt.AddDays(14); // default 14-day payment term

            return new InvoiceDto
            {
                SaleId = sale.Id,
                InvoiceNumber = sale.InvoiceNumber,
                IssuedAt = sale.CreatedAt,
                DueDate = dueDate,
                PaymentMethod = sale.PaymentMethod.ToString(),
                Status = sale.Status.ToString(),
                Subtotal = sale.Subtotal,
                Tax = sale.Tax,
                Total = sale.Total,
                Customer = new InvoiceCustomerDto
                {
                    Name = sale.Customer.Name,
                    Phone = sale.Customer.Phone,
                    Address = sale.Customer.Address
                },
                Company = new InvoiceCompanyDto
                {
                    Name = company?.Name ?? "William Metal",
                    Address = company?.Address,
                    Phone = company?.Phone,
                    Email = company?.Email,
                    TaxRate = company?.TaxRate ?? 0,
                    Currency = company?.Currency ?? "MAD"
                },
                Items = sale.Items.Select(item => new InvoiceItemDto
                {
                    ProductName = item.Variant.Product.NameAr,
                    VariantName = item.Variant.Specification,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    LineTotal = item.TotalPrice
                }).ToList()
            };
        }

        private async Task<Models.CompanySettings?> GetCompanySettingsAsync()
        {
            return await _context.CompanySettings.FirstOrDefaultAsync();
        }

        private static byte[] BuildInvoicePdf(InvoiceDto invoice, byte[]? logoBytes)
        {
            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(TextStyle.Default.FontSize(10));

                    page.Header().Row(row =>
                    {
                        // Logo (optional)
                        if (logoBytes != null && logoBytes.Length > 0)
                        {
                            row.ConstantItem(90)
                                .AlignMiddle()
                                .AlignLeft()
                                .Height(55)
                                .Image(logoBytes);
                        }

                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(t => t.Span(invoice.Company.Name).FontSize(18).Bold());
                            if (!string.IsNullOrWhiteSpace(invoice.Company.Address))
                                col.Item().Text(invoice.Company.Address);
                            if (!string.IsNullOrWhiteSpace(invoice.Company.Phone))
                                col.Item().Text($"Téléphone : {invoice.Company.Phone}");
                            if (!string.IsNullOrWhiteSpace(invoice.Company.Email))
                                col.Item().Text($"Email : {invoice.Company.Email}");
                        });

                        row.ConstantItem(180).Column(col =>
                        {
                            col.Item().Text(t => t.Span($"Facture N° {invoice.InvoiceNumber}").FontSize(16).Bold());
                            col.Item().Text($"Date : {invoice.IssuedAt:yyyy-MM-dd}");
                            if (invoice.DueDate.HasValue)
                                col.Item().Text($"Échéance : {invoice.DueDate:yyyy-MM-dd}");
                            col.Item().Text($"Statut : {invoice.Status}");
                            col.Item().Text($"Paiement : {invoice.PaymentMethod}");
                        });
                    });

                    page.Content().Column(col =>
                    {
                        col.Spacing(12);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text(t =>
                            {
                                t.Span("Client : ").SemiBold();
                                t.Span(invoice.Customer.Name);
                            });
                            row.RelativeItem().AlignRight().Text(t =>
                            {
                                t.Span("Devise : ").SemiBold();
                                t.Span(invoice.Company.Currency);
                            });
                        });

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                if (!string.IsNullOrWhiteSpace(invoice.Customer.Phone))
                                    c.Item().Text($"Téléphone : {invoice.Customer.Phone}");
                                if (!string.IsNullOrWhiteSpace(invoice.Customer.Address))
                                    c.Item().Text($"Adresse : {invoice.Customer.Address}");
                            });
                            row.RelativeItem();
                        });

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).Text("Produit");
                                header.Cell().Element(HeaderCell).Text("Variante");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Qté");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Total");
                            });

                            foreach (var item in invoice.Items)
                            {
                                table.Cell().Element(Cell).Text(item.ProductName);
                                table.Cell().Element(Cell).Text(item.VariantName);
                                table.Cell().Element(Cell).AlignRight().Text(item.Quantity.ToString());
                                table.Cell().Element(Cell).AlignRight().Text(FormatMoney(item.LineTotal, invoice.Company.Currency));
                            }
                        });

                        col.Item().Row(row =>
                        {
                            row.RelativeItem();
                            row.ConstantItem(200).Column(c =>
                            {
                                c.Item().Row(r =>
                                {
                                    r.RelativeItem().Text(t => t.Span("Sous-total").SemiBold());
                                    r.ConstantItem(90).AlignRight().Text(FormatMoney(invoice.Subtotal, invoice.Company.Currency));
                                });
                                c.Item().Row(r =>
                                {
                                    r.RelativeItem().Text(t => t.Span($"TVA ({invoice.Company.TaxRate:0.#}%)").SemiBold());
                                    r.ConstantItem(90).AlignRight().Text(FormatMoney(invoice.Tax, invoice.Company.Currency));
                                });
                                c.Item().Row(r =>
                                {
                                    r.RelativeItem().Text(t => t.Span("Total").Bold());
                                    r.ConstantItem(90).AlignRight().Text(t => t.Span(FormatMoney(invoice.Total, invoice.Company.Currency)).FontSize(12).Bold());
                                });
                            });
                        });
                    });

                    page.Footer().AlignRight().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });

            return doc.GeneratePdf();

            static IContainer HeaderCell(IContainer container) =>
                container.DefaultTextStyle(TextStyle.Default.SemiBold()).PaddingVertical(4).BorderBottom(1).BorderColor(Colors.Grey.Darken2);

            static IContainer Cell(IContainer container) =>
                container.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3);

            static string FormatMoney(decimal amount, string currency) => $"{amount:0.00} {currency}";
        }
    }
}
