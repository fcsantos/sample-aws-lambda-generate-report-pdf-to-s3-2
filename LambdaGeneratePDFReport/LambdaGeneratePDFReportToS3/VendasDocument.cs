using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace LambdaGeneratePDFReportToS3;

public partial class Function
{
    public class VendasDocument : IDocument
    {
        private List<DadosVenda> Vendas { get; }
        private DateTime StartDate { get; }
        private DateTime EndDate { get; }

        public VendasDocument(List<DadosVenda> vendas, DateTime startDate, DateTime endDate)
        {
            Vendas = vendas;
            StartDate = startDate;
            EndDate = endDate;
        }

        public DocumentMetadata GetMetadata()
        {
            return DocumentMetadata.Default;
        }

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(50);

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().Element(ComposeFooter);
                });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("Relatório de Vendas")
                        .Bold()
                        .FontSize(20);

                    column.Item().Text($"Período: {StartDate:dd/MM/yyyy} até {EndDate:dd/MM/yyyy}")
                        .FontSize(12);
                });
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(20).Table(table =>
            {
                // cabeçalho da tabela
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Text("Data").Bold();
                    header.Cell().Text("Produto").Bold();
                    header.Cell().AlignRight().Text("Quantidade").Bold();
                    header.Cell().AlignRight().Text("Valor Total").Bold();
                });

                // dados
                foreach (var venda in Vendas)
                {
                    table.Cell().Text(venda.Data.ToString("dd/MM/yyyy"));
                    table.Cell().Text(venda.Produto);
                    table.Cell().AlignRight().Text(venda.Quantidade.ToString("N0"));
                    table.Cell().AlignRight().Text($"R$ {venda.ValorTotal:N2}");
                }

                // totais
                table.Cell().ColumnSpan(3).AlignRight().Text("Total:").Bold();
                table.Cell().AlignRight().Text($"R$ {Vendas.Sum(x => x.ValorTotal):N2}").Bold();
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Text($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                    .FontSize(10);
                row.RelativeItem().AlignRight().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        }
    }
}