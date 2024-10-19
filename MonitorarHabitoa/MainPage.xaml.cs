using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;

namespace MonitorarHabitoa;

public partial class MainPage : ContentPage
    {
        // Lista que armazena os hábitos
        List<Habito> habitos = new List<Habito>();

        public MainPage()
        {
            InitializeComponent();
            CarregarHabitos();
            ListaDeHabitos.ItemsSource = habitos; // Exibe a lista de hábitos na interface
            ResetarProgressoDiario(); // Reseta o progresso diariamente
        }

        // Adiciona um novo hábito quando o botão é clicado
        private async void AoAdicionarHabito(object sender, EventArgs e)
        {
            string tituloHabito = await DisplayPromptAsync("Novo Hábito", "Digite o nome do hábito:");
            string frequenciaInput = await DisplayPromptAsync("Meta Diária", "Quantas vezes por dia deseja realizar esse hábito?");
            
            if (!string.IsNullOrWhiteSpace(tituloHabito) && int.TryParse(frequenciaInput, out int frequencia) && frequencia > 0)
            {
                var novoHabito = new Habito { Titulo = tituloHabito, Frequencia = frequencia, Progresso = 0 };
                habitos.Add(novoHabito);

                ListaDeHabitos.ItemsSource = null;
                ListaDeHabitos.ItemsSource = habitos; // Atualiza a lista de hábitos na interface

                SalvarHabitos(); // Salva os hábitos no armazenamento local
            }
        }

        // Marca o hábito como concluído
        private void AoConcluirHabito(object sender, EventArgs e)
        {
            var botao = (Button)sender;
            var habito = (Habito)botao.BindingContext;

            if (habito.Progresso < habito.Frequencia)
            {
                habito.Progresso++;
                DisplayAlert("Hábito Concluído", $"Você completou: {habito.Titulo} ({habito.Progresso}/{habito.Frequencia})", "OK");
                SalvarHabitos(); // Salva o progresso atualizado
            }
            else
            {
                DisplayAlert("Meta Atingida", $"Você já atingiu a meta diária de {habito.Frequencia} vezes para o hábito: {habito.Titulo}.", "OK");
            }
        }

        // Remove o hábito
        private void AoRemoverHabito(object sender, EventArgs e)
        {
            var botao = (Button)sender;
            var habito = (Habito)botao.BindingContext;

            habitos.Remove(habito);

            ListaDeHabitos.ItemsSource = null;
            ListaDeHabitos.ItemsSource = habitos; // Atualiza a lista de hábitos na interface

            SalvarHabitos(); // Salva a nova lista sem o hábito removido
        }

        // Carrega os hábitos salvos anteriormente
        private void CarregarHabitos()
        {
            int i = 0;
            while (Preferences.ContainsKey($"habito_titulo_{i}"))
            {
                var titulo = Preferences.Get($"habito_titulo_{i}", string.Empty);
                var progresso = Preferences.Get($"habito_progresso_{i}", 0);
                var frequencia = Preferences.Get($"habito_frequencia_{i}", 1);

                habitos.Add(new Habito { Titulo = titulo, Progresso = progresso, Frequencia = frequencia });
                i++;
            }
        }

        // Salva os hábitos no armazenamento local
        private void SalvarHabitos()
        {
            for (int i = 0; i < habitos.Count; i++)
            {
                Preferences.Set($"habito_titulo_{i}", habitos[i].Titulo);
                Preferences.Set($"habito_progresso_{i}", habitos[i].Progresso);
                Preferences.Set($"habito_frequencia_{i}", habitos[i].Frequencia);
            }

            // Remove qualquer hábito extra que foi excluído
            int indice = habitos.Count;
            while (Preferences.ContainsKey($"habito_titulo_{indice}"))
            {
                Preferences.Remove($"habito_titulo_{indice}");
                Preferences.Remove($"habito_progresso_{indice}");
                Preferences.Remove($"habito_frequencia_{indice}");
                indice++;
            }
        }

        // Reseta o progresso dos hábitos diariamente
        private void ResetarProgressoDiario()
        {
            var ultimaDataReset = Preferences.Get("ultima_data_reset", DateTime.MinValue);
            if (ultimaDataReset.Date < DateTime.Today)
            {
                foreach (var habito in habitos)
                {
                    habito.Progresso = 0;
                }
                Preferences.Set("ultima_data_reset", DateTime.Today);
                SalvarHabitos(); // Salva os hábitos com o progresso zerado
            }
        }
    }

    // Classe que define o objeto Habito
    public class Habito
    {
        public string Titulo { get; set; }
        public int Progresso { get; set; }
        public int Frequencia { get; set; }

        // Propriedade que exibe o progresso no formato "x/y"
        public string ProgressoFormatado => $"{Progresso}/{Frequencia}";
    }

