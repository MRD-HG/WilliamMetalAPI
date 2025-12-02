using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WilliamMetalAPI.Data;
using WilliamMetalAPI.DTOs;

namespace WilliamMetalAPI.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly WilliamMetalContext _context;

        public InvoiceService(WilliamMetalContext context)
        {
            _context = context;
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

            var pdfBytes = BuildInvoicePdf(invoice);
            return (pdfBytes, invoice.InvoiceNumber);
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

        private static byte[] BuildInvoicePdf(InvoiceDto invoice)
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
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(t => t.Span(invoice.Company.Name).FontSize(18).Bold());
                            if (!string.IsNullOrWhiteSpace(invoice.Company.Address))
                                col.Item().Text(invoice.Company.Address);
                            if (!string.IsNullOrWhiteSpace(invoice.Company.Phone))
                                col.Item().Text($"Phone: {invoice.Company.Phone}");
                            if (!string.IsNullOrWhiteSpace(invoice.Company.Email))
                                col.Item().Text($"Email: {invoice.Company.Email}");
                        });

                        row.ConstantItem(180).Column(col =>
                        {
                            col.Item().Text(t => t.Span($"Invoice #{invoice.InvoiceNumber}").FontSize(16).Bold());
                            col.Item().Text($"Issued: {invoice.IssuedAt:yyyy-MM-dd}");
                            if (invoice.DueDate.HasValue)
                                col.Item().Text($"Due: {invoice.DueDate:yyyy-MM-dd}");
                            col.Item().Text($"Status: {invoice.Status}");
                            col.Item().Text($"Payment: {invoice.PaymentMethod}");
                        });
                    });

                    page.Content().Column(col =>
                    {
                        col.Spacing(12);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text(t =>
                            {
                                t.Span("Bill To: ").SemiBold();
                                t.Span(invoice.Customer.Name);
                            });
                            row.RelativeItem().AlignRight().Text(t =>
                            {
                                t.Span("Currency: ").SemiBold();
                                t.Span(invoice.Company.Currency);
                            });
                        });

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                if (!string.IsNullOrWhiteSpace(invoice.Customer.Phone))
                                    c.Item().Text($"Phone: {invoice.Customer.Phone}");
                                if (!string.IsNullOrWhiteSpace(invoice.Customer.Address))
                                    c.Item().Text($"Address: {invoice.Customer.Address}");
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
                                header.Cell().Element(HeaderCell).Text("Product");
                                header.Cell().Element(HeaderCell).Text("Variant");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Qty");
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
                                    r.RelativeItem().Text(t => t.Span("Subtotal").SemiBold());
                                    r.ConstantItem(90).AlignRight().Text(FormatMoney(invoice.Subtotal, invoice.Company.Currency));
                                });
                                c.Item().Row(r =>
                                {
                                    r.RelativeItem().Text(t => t.Span($"Tax ({invoice.Company.TaxRate:0.#}%)").SemiBold());
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
                        x.Span(" of ");
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
