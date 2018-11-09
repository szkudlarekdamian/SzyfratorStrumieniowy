using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading;
using System.Runtime.CompilerServices;
using System.IO;
using System.ComponentModel;

namespace SzyfratorStrumieniowy
{
    public partial class MainWindow : Window
    {
        private int howMuchRegisters = 3,
            maximumLengthOfRegister = 20,
            keyLength = 20000;
        private bool stop = false;
        private String keyGlobal = "";

        private static Random random = new Random();
        private List<int> registersLengths = new List<int>();
        private List<KeyValuePair<List<String>, List<String>>> registers = new List<KeyValuePair<List<String>, List<String>>>();
        private List<KeyValuePair<List<String>, List<String>>> registersStepByStep = new List<KeyValuePair<List<String>, List<String>>>();
        private List<KeyValuePair<List<String>, List<String>>> registersBackup = new List<KeyValuePair<List<String>, List<String>>>();
        private List<KeyValuePair<short, List<String>>> perfectPolynomians = new List<KeyValuePair<short, List<String>>>();
        private String helpText = "Autor: Damian Szkudlarek\nRodzaj generatora: Generator progowy\n\nZasada działania generatora progowego opiera się na wspólnej pracy, nieparzystej liczby rejestrów LSFR.\nPrzykład działania:\nZałóżmy że mamy 3 rejestry o różnej długości.W każdej iteracji bity rejestrów przesuwane są o jeden w prawo, tak, że ostatni bit zostaje wypchnięty, a na miejsce pierwszego dostaje się reszta z dzielenia przez dwa wyniku mnożenia rejestru z wielomianem.\nW kolejnym kroku należy zliczyć ile rejestrów wypchnęło bit prawdy. Jeśli suma ta przekracza połowę ilości rejestrów to do klucza dodajemy 1, w przeciwnym przypadku 0.\nWizualizacja przykładu działania generatora znajduje się w zakładce Krok po kroku\n\nWażne!\nGdy długości rejestrów są względnie pierwsze,a wielomiany gałęzi sprzężenia zwrotnego pierwotne, to okres tego generatora jest maksymalny.\nSzum generatora można zauważyć przy długości rejestrów powyżej 10. Wcześniej zauważyć można powatarzający się wzór.\n\n\nFunkcje programu:\n\t-Generowanie rejestrów LSFR,\n\t\t*Ręczne ustawienie parametrów:\n\t\t\t>Liczba rejestrów do wygenerowania,\n\t\t\t>Maksymalna długość pojedynczego rejestru,\n\t\t\t>Wybór pomiędzy losowymi wielomianami, a pierwotnymi*,\n\t-Wizualizacja rejestrów(x) i ich wielomianów(a),\n\t-Wybór długości klucza do wygenerowania,\n\t-Przycisk STOP, przerywający generowanie klucza,\n\t-Pomiar liczby wygenerowanych znaków oraz czasu, w jakim się to stało,\n\t-Zapis wygenerowanego klucza do pliku tekstowego lub binarnego,\n\t-Zapis rejestrów do pliku\n\t-Wczytanie rejestrów z pliku\n\n*Pierwotne wielomiany przedstawione zostały w dodatku w książce Schneier B. Kryptografia dla praktyków\n\n\nUwagi odnośnie programu:\n\t-Generowanie rejestrów nie wykonuje się automatycznie. Po zmianie parametrów należy każdorazowo wcisnąć przycisk Generuj rejestry.\n\t-Program obsługuje tylko pliki tekstowe i binarne.\n\t-Maksymalna długość rejestru została ograniczona w celach prezentacyjnych.\n\t-W programie rejestr i wielomian mają taką samą długość, dodatkowa jedynka przed wielomianem nie wpływa na obliczenia i ma funkcję tylko symboliczną.\n\t-Przy wczytaniu rejestrów z pliku:\n\t\t*Jeśli długość rejestru i wielomianu różni się lub wielomian nie został podany, to wielomian zostaje zastąpiony odpowiadającym długości rejestru, wielomianem pierwotnym.\n\t\t*Jeśli w rejestrze znajdują się same zera, to ostatnie zero zostaje zamienione na jedynkę.\n\t\t*Jeśli w wielomianie najstarszy bit nie jest jedynką to zostaje zamieniony na jedynkę.\n\t\t*Linie pliku, które nie zostały zapisane w odpowiednim formacie zostają pominięte.\n\t\t*Jeśli w pliku zapisano mniej niż 3 poprawne rejestry, lub parzystą liczbę poprawnych rejestrów to wczytywanie zakończy się niepowodzeniem.\n\t\t*Jeśli w pliku dwa rejestry mają taką samą długość to wczytywanie pliku zakończy się niepowodzeniem.\n\nPrzykład obsługi programu - generowanie rejestrów\nKrok 1.\n\tWybierz liczbę rejestrów do wygenerowania.\nKrok 2.\n\tWybierz maksymalną długość rejestru.\nKrok 3.\n\tWybierz rodzaj wielomianu, preferowany Pierwotne.\nKrok 4. \n\tNaciśnij przycisk Generuj rejestry.\n\n\n\n\nPrzykład obsługi programu - generowanie klucza \nKrok 1.\n\tGdy wygenerowano lub wczytano już rejestry, należy wpisać długość klucza.\nKrok 2.\n\tNaciśnij przycisk Generuj klucz.\nKrok 3.\n\tJeśli generowanie klucza trwa za długo, wciśnij przycisk STOP - przerwie to pracę generatora i wyświetli obok część klucza.\nKrok 4. \n\tZapisz klucz do pliku.\nKrok 5.\n\tZapisz rejestry do pliku.\n\n\n\n\nPrzykład obsługi programu - wczytanie rejestów\nKrok 1.\n\tNaciśnij przycisk Wczytaj rejestry.\nKrok 2.\n\tWybierz plik do wczytania. Plik powinien być zapisany w odpowiednim formacie 'rejestr; wielomian' - taki jak przy zapisie rejestrów do pliku.\nKrok 3.\n\tObejrzyj rejestry poniżej.\n";


        public MainWindow()
        {
            CreatePerfectPolynomians();
            InitializeComponent();
            help.Text = helpText;
        }
        #region Losowanie
        public static List<int> GenerateRandom(int count, int min, int max)
        {
            if (max <= min || count < 0 ||

                    (count > max - min && max - min > 0))
            {

                throw new ArgumentOutOfRangeException("Range " + min + " to " + max +
                        " (" + ((Int64)max - (Int64)min) + " values), or count " + count + " is illegal");
            }


            HashSet<int> candidates = new HashSet<int>();

            for (int top = max - count; top < max; top++)
            {
                if (!candidates.Add(random.Next(min, top + 1)))
                {
                    candidates.Add(top);
                }
            }


            List<int> result = candidates.ToList();
            for (int i = result.Count - 1; i > 0; i--)
            {
                int k = random.Next(i + 1);
                int tmp = result[k];
                result[k] = result[i];
                result[i] = tmp;
            }
            return result;
        }
        #endregion

        #region Okienka informacyjne
        private void showAlert(String text)
        {
            string caption = "Uwaga";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Information;
            MessageBox.Show(text, caption, button, icon);
        }
        #endregion

        #region Stałe wielomiany
        private void CreatePerfectPolynomians()
        {
            perfectPolynomians.Add(new KeyValuePair<short, List<String>>(3, CreatePolynomian(new List<short>() { 4, 1, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, List<String>>(4, CreatePolynomian(new List<short>() { 5, 2, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, List<String>>(5, CreatePolynomian(new List<short>() { 6, 1, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, List<String>>(6, CreatePolynomian(new List<short>() { 7, 1, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, List<String>>(7, CreatePolynomian(new List<short>() { 8, 4, 3, 2, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, List<String>>(8, CreatePolynomian(new List<short>() { 9, 4, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, List<String>>(9, CreatePolynomian(new List<short>() { 10, 3, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, List<String>>(10, CreatePolynomian(new List<short>() { 11, 2, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, List<String>>(11, CreatePolynomian(new List<short>() { 12, 6, 4, 1, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, List<String>>(12, CreatePolynomian(new List<short>() { 13, 4, 3, 1, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, List<String>>(13, CreatePolynomian(new List<short>() { 14, 5, 3, 1, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, List<String>>(14, CreatePolynomian(new List<short>() { 15, 1, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, List<String>>(15, CreatePolynomian(new List<short>() { 16, 5, 3, 2, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, List<String>>(16, CreatePolynomian(new List<short>() { 17, 3, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, List<String>>(17, CreatePolynomian(new List<short>() { 18, 5, 2, 1, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, List<String>>(18, CreatePolynomian(new List<short>() { 19, 5, 2, 1, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, List<String>>(19, CreatePolynomian(new List<short>() { 20, 3, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, List<String>>(20, CreatePolynomian(new List<short>() { 21, 2, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, List<String>>(21, CreatePolynomian(new List<short>() { 22, 1, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, List<String>>(22, CreatePolynomian(new List<short>() { 23, 5, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, List<String>>(23, CreatePolynomian(new List<short>() { 24, 4, 3, 1, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, List<String>>(24, CreatePolynomian(new List<short>() { 25, 3, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, List<String>>(25, CreatePolynomian(new List<short>() { 26, 6, 2, 1, 0 })));
        }

        private List<String> CreatePolynomian(List<short> list)
        {
            short length = (short)(list.ElementAt(0) - 1);
            String returnString = "";
            for (short i = 0; i < (length - list.Count() + 1); i++)
            {
                returnString += "0";
            }
            List<short> dummy = list;
            dummy.RemoveAt(0);
            foreach (short index in dummy)
            {
                short position = (short)(length - (index + 1));
                returnString = returnString.Insert(position, "1");
            }
            return returnString.Select(x => x.ToString()).ToList();

        }
        #endregion

        #region Utworzenie rejestru
        private KeyValuePair<List<String>, List<String>> CreateRegister()
        {
            List<String> registerContent = new List<String>();
            List<String> polynomial = new List<String>();
            int randomNumber = -1;
            do
            {
                randomNumber = GenerateRandom(1, 3, maximumLengthOfRegister + 1)[0];
            } while (registersLengths.Contains(randomNumber));
            registersLengths.Add(randomNumber);

            for (int i = 0; i < randomNumber - 1; i++)
            {
                polynomial.Add(GenerateRandom(1, 0, 2)[0].ToString());
            }
            for (int i = 0; i < randomNumber; i++)
                registerContent.Add(GenerateRandom(1, 0, 2)[0].ToString());

            if (!registerContent.Contains("1"))
                registerContent[GenerateRandom(1, 0, registerContent.Count())[0]] = "1";
            polynomial.Add("1");


            return new KeyValuePair<List<String>, List<String>>(registerContent, polynomial);
        }
        #endregion

        #region Zamiany z bin na ASCII i odwrotnie
        private byte[] GetBytesFromBinaryString(String binary)
        {
            int numOfBytes = binary.Length / 8;
            byte[] bytes = new byte[numOfBytes];
            for (int i = 0; i < numOfBytes; ++i)
            {
                bytes[i] = Convert.ToByte(binary.Substring(8 * i, 8), 2);
            }
            return bytes;
        }
        private String GetBinaryStringFromString(String normal)
        {
            String returnString = "";
            foreach (var letter in normal)
            {
                var bits = Convert.ToByte(Convert.ToByte(letter));
                returnString +=Convert.ToString(bits,2);
            }
            var howMuchZeros = 8 - returnString.Length;
           for(int i = 0; i < howMuchZeros; i++)
            {
                returnString = returnString.Insert(0, "0");
            }
            return returnString;
        }
        #endregion

        #region Wygenerowanie nieparzystej liczby rejestrów
        private void MakeRegisters()
        {
            registers.Clear();
            registersLengths.Clear();
            for (int i = 0; i < howMuchRegisters; i++)
                registers.Add(CreateRegister());
            registersBackup = CopyList(registers);

        }
        #endregion

        #region Głęboka kopia listy

        private List<KeyValuePair<List<string>, List<string>>> CopyList(List<KeyValuePair<List<string>, List<string>>> list)
        {
            List<KeyValuePair<List<string>, List<string>>> temp = new List<KeyValuePair<List<string>, List<string>>>();
            for (int i = 0; i < list.Count(); i++)
                temp.Add(new KeyValuePair<List<string>, List<string>>(list[i].Key.ToList(), list[i].Value.ToList()));
            return temp;
        }

        #endregion

        #region Wygenerowanie klucza
        private String GenerateKey()
        {
            String key = "";
            registers = CopyList(registersBackup);
            int sum = 0;
            int count = registers.Count;
            int countDivided = count / 2;
            for (int i = 0; i < keyLength; i++)
            {
                sum = 0;
                for (int j = 0; j < count; j++)
                {
                    var temp = registers[j];
                    sum += IterateOverRegister(ref temp);
                    registers[j] = temp;
                }
                if (sum > countDivided) key += "1";
                else key += "0";

                if (stop == true)
                {
                    break;
                }

            }
            return key;
        }

        #endregion

        #region Jedna iteracja po rejestrze
        private int IterateOverRegister(ref KeyValuePair<List<String>, List<String>> register)
        {
            int sum = 0;
            for (int i = 0; i < register.Key.Count; i++)
            {
                if (register.Key[i] == "1" && register.Value[i] == "1")
                    sum += 1;
            }
            int pushedValue = int.Parse(register.Key.Last());
            //List<String> temp = new List<String>();
            //temp.Add((sum % 2).ToString());
            //temp = register.Key.GetRange(0, register.Key.Count-1);
            register.Key.RemoveAt(register.Key.Count - 1);
            register.Key.Insert(0, (sum % 2).ToString());
            //register = new KeyValuePair<List<String>, List<String>>(temp, register.Value);
            return pushedValue;
        }

        #endregion

        #region EventyXAMLA
        private void howMuchRegistersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (howMuchRegistersComboBox.Items.Count > 1)
            {
                int howM = int.Parse(howMuchRegistersComboBox.SelectedItem.ToString().Last().ToString());
                slValue.Minimum = howM + 2;
            }
        }

        private void maximumLengthOfRegisterTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            System.Text.RegularExpressions.Regex _regex = new System.Text.RegularExpressions.Regex("[^0-9]+$");
            e.Handled = _regex.IsMatch(e.Text);
        }

        private void keyLengthTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            System.Text.RegularExpressions.Regex _regex = new System.Text.RegularExpressions.Regex("[^0-9]+$");
            e.Handled = _regex.IsMatch(e.Text);
        }

        private void keyLengthTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TextBox)sender).Text = string.Concat(((TextBox)sender).Text.Where(x => char.IsDigit(x)).Select(x => x));
        }

        private void maximumLengthOfRegisterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            maximumLengthOfRegisterTextBox.Text = string.Concat(maximumLengthOfRegisterTextBox.Text.Where(x => char.IsDigit(x)).Select(x => x));
        }

        private void generateRegisters_Click(object sender, RoutedEventArgs e)
        {
            System.Text.RegularExpressions.Regex _regex = new System.Text.RegularExpressions.Regex("[^0-9]+$");
            if (!_regex.IsMatch(maximumLengthOfRegisterTextBox.Text))
            {
                howMuchRegisters = int.Parse(howMuchRegistersComboBox.SelectedItem.ToString().Last().ToString());
                maximumLengthOfRegister = int.Parse(maximumLengthOfRegisterTextBox.Text);
                MakeRegisters();

                if (perfectPolynomiansRadioButton.IsChecked == true)
                {
                    for (short p = 0; p < registers.Count(); p++)
                    {
                        var p1 = registers[p].Key;
                        var p2 = perfectPolynomians.Where(z => z.Key == p1.Count()).Select(x => x.Value).ToList();
                        registers.RemoveAt(p);
                        registers.Insert(p, new KeyValuePair<List<string>, List<string>>(p1, p2[0]));

                    }
                }
                registersBackup = CopyList(registers);

                fillTheRegistersStackPanel();

                if(textToBeCiphered.Text.Length < 100000) {
                    keyLengthTextBox.Text = textToBeCiphered.Text.Length.ToString();
                }
                else
                {
                    //DAĆ TUTAJ ALERTA ŻE BEDZIE SEI DŁUGO ROBIĆ I DO WYBORU, ALBO CZEKAMY ALBO POWIELAMY KLUCZ
                    keyLengthTextBox.Text = textToBeCiphered.Text.Length.ToString();
                }
                keyLengthWrapPanel.Visibility = Visibility.Visible;
                generateKeyBtn.Visibility = Visibility.Visible;
            }
            else
            {
                showAlert("Nieprawidłowo uzupełniono pole: 'Maksymalna dlugość rejestru'");
            }
        }

        private void fillTheRegistersStackPanel()
        {
            generatedRegistersStackPanel.Children.Clear();

            int i = 1, j = 1;

            foreach (var register in registers)
            {
                StackPanel stack = new StackPanel { Name = "stackPanel" + i.ToString(), Margin = new Thickness(0, 2, 0, 2) };
                WrapPanel wrapX = new WrapPanel { Name = "wrapPanelX" + i.ToString(), Margin = new Thickness(0, 2, 0, 0) };
                WrapPanel wrapA = new WrapPanel { Name = "wrapPanelA" + i.ToString(), Margin = new Thickness(0, 2, 0, 0) };
                stack.Children.Add(new Label { Content = "LFSR " + i.ToString(), Margin = new Thickness(0, 0, 0, 0), FontWeight = FontWeights.DemiBold });
                wrapX.Children.Add(new Label { Content = "x:", Margin = new Thickness(0, 0, 14, 0) });
                wrapA.Children.Add(new Label { Content = "a:", Margin = new Thickness(0, 0, 0, 0) });
                wrapA.Children.Add(new TextBox { Name = "textboxA" + i.ToString() + "Model", Margin = new Thickness(1, 0, 0, 0), FontSize = 9, Text = "1", Width = 13, Height = 13, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, IsReadOnly = true });

                foreach (var xi in register.Key)
                {
                    wrapX.Children.Add(new TextBox { Name = "textboxX" + i.ToString() + j.ToString(), Margin = new Thickness(0.4, 0, 0, 0), FontSize = 9, Text = xi.ToString(), Width = 13, Height = 13, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, IsReadOnly = true });
                    j++;
                }
                j = 1;
                foreach (var ai in register.Value)
                {
                    wrapA.Children.Add(new TextBox { Name = "textboxA" + i.ToString() + j.ToString(), Margin = new Thickness(0.4, 0, 0, 0), FontSize = 9, Text = ai.ToString(), Width = 13, Height = 13, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, IsReadOnly = true });
                    j++;
                }
                j = 1;
                i++;
                stack.Children.Add(wrapX);
                stack.Children.Add(wrapA);
                stack.Children.Add(new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 1, 0, 0), Margin = new Thickness(15, 2, 15, 2) });
                generatedRegistersStackPanel.Children.Add(stack);
            }
        }

        private void stopKeyBtn_Click(object sender, RoutedEventArgs e)
        {
            stop = true;
        }

        private void keySaveButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                DefaultExt = ".bin",
                Filter = "Pliki binarne (.bin)|.bin|Pliki tekstowe (.txt)|*.txt"
            };

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true && keyGlobal != "")
            {
                if (dlg.FileName.Substring(dlg.FileName.Length - 3, 3) == "txt")
                    File.WriteAllText(dlg.FileName, keyGlobal, Encoding.GetEncoding("Windows-1250"));
                else
                {
                    int numOfBytes = keyGlobal.Length / 8;
                    byte[] bytes = new byte[numOfBytes];
                    for (int i = 0; i < numOfBytes; ++i)
                    {
                        bytes[i] = Convert.ToByte(keyGlobal.Substring(8 * i, 8), 2);
                    }
                    File.WriteAllBytes(dlg.FileName, bytes);
                }
            }
            else
                showAlert("Błąd przy zapisie do pliku.");
        }

        private void infoSaveButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                DefaultExt = ".txt",
                Filter = "Pliki tekstowe (.txt)|*.txt"
            };
            String returnValue = "";
            foreach (var register in registers)
            {
                returnValue += String.Concat(String.Concat(register.Key), ";", String.Concat(register.Value), Environment.NewLine);
            }

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true && returnValue != "")
                File.WriteAllText(dlg.FileName, returnValue, Encoding.GetEncoding("Windows-1250"));
            else
                showAlert("Błąd przy zapisie do pliku.");
        }

        private void loadRegisters_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".txt",
                Filter = "Pliki tekstowe (.txt)|*.txt"
            };
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                String[] xa;
                String x, a;
                List<String> aL = new List<String>(), xL = new List<String>();
                System.Text.RegularExpressions.Regex _regex = new System.Text.RegularExpressions.Regex("[0-1]+;[0-1]*");
                var lines = File.ReadLines(dlg.FileName);
                if (lines.Count() % 2 == 0 || lines.Count() < 3) { showAlert("Parzysta lub mniejsza niż trzy liczba rejestrów."); return; }
                registers.Clear();
                int validLines = 0;
                List<short> lengths = new List<short>();

                foreach (var line in lines)
                {
                    if (!_regex.IsMatch(line)) continue;
                    xa = line.Split(';');
                    x = xa[0];
                    a = xa[1];
                    if (lengths.Contains((short)x.Length))
                    {
                        registers = CopyList(registersBackup);
                        showAlert("Rejestry o takiej samej długości.");
                        return;
                    }
                    else lengths.Add((short)x.Length);

                    if (!x.Contains('1')) { x.Remove(0, 1); x += '1'; }
                    if (a == "" || a.Length != x.Length) aL = perfectPolynomians.Where(z => z.Key == x.Length).Select(y => y.Value).ToList()[0];
                    else { if (!a.Last().Equals('1')) { a.Remove(a.Length - 1, 0); a += '1'; } aL = a.Select(y => y.ToString()).ToList(); }
                    xL = x.Select(y => y.ToString()).ToList();
                    registers.Add(new KeyValuePair<List<string>, List<string>>(xL, aL));

                    validLines++;
                }
                if (validLines % 2 == 0 || validLines < 3)
                {
                    registers = CopyList(registersBackup);
                    showAlert("Parzysta lub mniejsza niż trzy liczba POPRAWNYCH rejestrów.");
                    return;
                }
                registersBackup.Clear();
                registersBackup = CopyList(registers);
                fillTheRegistersStackPanel();
                keyLengthWrapPanel.Visibility = Visibility.Visible;
                generateKeyBtn.Visibility = Visibility.Visible;
            }
        }

        private void b1_Click(object sender, RoutedEventArgs e)
        {
            if (!t1a.Text.Contains("1") || !t1b.Text.Contains("1") || !t1c.Text.Contains("1"))
            {
                String message = "Rejestr numer: { ";
                if (!t1a.Text.Contains("1")) message += "1 ";
                if (!t1b.Text.Contains("1")) message += "2 ";
                if (!t1c.Text.Contains("1")) message += "3 ";
                message += "} zawiera same zera lub jest pusty.";
                showAlert(message);
                return;
            }
            else if (t1a.Text.Length == 3 && t1b.Text.Length == 4 && t1c.Text.Length == 5)
            {
                registersStepByStep.Clear();
                registersStepByStep.Add(new KeyValuePair<List<string>, List<string>>(t1a.Text.Select(x => x.ToString()).ToList(), perfectPolynomians.Where(x => x.Key == t1a.Text.Count()).Select(x => x.Value).ToList()[0]));
                registersStepByStep.Add(new KeyValuePair<List<string>, List<string>>(t1b.Text.Select(x => x.ToString()).ToList(), perfectPolynomians.Where(x => x.Key == t1b.Text.Count()).Select(x => x.Value).ToList()[0]));
                registersStepByStep.Add(new KeyValuePair<List<string>, List<string>>(t1c.Text.Select(x => x.ToString()).ToList(), perfectPolynomians.Where(x => x.Key == t1c.Text.Count()).Select(x => x.Value).ToList()[0]));
                fillTheRegistersStepByStepStackPanel();
                sp2.Visibility = Visibility.Visible;
            }
            else
                showAlert("Wprowadzono niepełną ilość bitów.");

        }

        private void b2_Click(object sender, RoutedEventArgs e)
        {
            int sum = 0;
            int count = registersStepByStep.Count;
            int countDivided = count / 2;
            for (int j = 0; j < count; j++)
            {
                var temp = registersStepByStep[j];
                sum += IterateOverRegister(ref temp);
                registersStepByStep[j] = temp;
            }
            if (sum > countDivided) stepByStepKeyTextBox.Text += "1";
            else stepByStepKeyTextBox.Text += "0";
            fillTheRegistersStepByStepStackPanel();

        }

        private void fillTheRegistersStepByStepStackPanel()
        {
            generatedRegistersStackPanelStepByStep.Children.Clear();

            int i = 1;
            short j = 0;
            short sum = 0;

            foreach (var register in registersStepByStep)
            {
                sum = 0;
                StackPanel stack = new StackPanel { Margin = new Thickness(0, 2, 0, 2) };
                WrapPanel wrapX = new WrapPanel { Margin = new Thickness(0, 2, 0, 0) };
                WrapPanel wrapA = new WrapPanel { Margin = new Thickness(0, 2, 0, 0) };
                stack.Children.Add(new Label { Content = "LFSR " + i.ToString(), Margin = new Thickness(0, 0, 0, 0), FontWeight = FontWeights.DemiBold });
                wrapX.Children.Add(new Label { Content = "x:", Margin = new Thickness(0, 0, 23, 0) });
                wrapA.Children.Add(new Label { Content = "a:", Margin = new Thickness(0, 0, 0, 0) });
                wrapA.Children.Add(new TextBox { Margin = new Thickness(3, 0, 0, 0), FontSize = 14, Text = "1", Width = 20, Height = 20, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, IsReadOnly = true });

                List<short> turnedOn = new List<short>();

                foreach (var ai in register.Value)
                {
                    if (ai == "1")
                    {
                        wrapA.Children.Add(new TextBox { Background = Brushes.Yellow, Margin = new Thickness(3, 0, 0, 0), FontSize = 14, Text = ai, Width = 20, Height = 20, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, IsReadOnly = true });
                        turnedOn.Add(j);
                    }
                    else wrapA.Children.Add(new TextBox { Margin = new Thickness(3, 0, 0, 0), FontSize = 14, Text = ai, Width = 20, Height = 20, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, IsReadOnly = true });
                    j++;
                }
                j = 0;
                foreach (var xi in register.Key)
                {
                    if (turnedOn.Contains(j)) wrapX.Children.Add(new TextBox { Background = Brushes.Yellow, Margin = new Thickness(3, 0, 0, 0), FontSize = 14, Text = xi, Width = 20, Height = 20, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, IsReadOnly = true });
                    else wrapX.Children.Add(new TextBox { Margin = new Thickness(3, 0, 0, 0), FontSize = 14, Text = xi, Width = 20, Height = 20, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, IsReadOnly = true });
                    j++;
                }
                j = 0;
                i++;

                for (int p = 0; p < register.Key.Count; p++)
                {
                    if (register.Key[p] == "1" && register.Value[p] == "1")
                        sum += 1;
                }

                ((TextBox)wrapX.Children[wrapX.Children.Count - 1]).Background = Brushes.PeachPuff;
                wrapX.Children.Add(new Label { Content = " Wypchnięty bit:", Margin = new Thickness(25 + (46 - ((i - 1) * 23)), 0, 0, 0) });
                wrapX.Children.Add(new TextBox { Margin = new Thickness(3, 0, 0, 0), Background = Brushes.PeachPuff, FontSize = 14, Text = ((TextBox)wrapX.Children[wrapX.Children.Count - 2]).Text, Width = 20, Height = 20, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, IsReadOnly = true });

                wrapA.Children.Add(new Label { Content = " XOR:", Margin = new Thickness(25 + (46 - ((i - 1) * 23)), 0, 0, 0) });
                wrapA.Children.Add(new TextBox { Margin = new Thickness(3, 0, 0, 0), FontSize = 14, Text = sum + "%2", Width = 40, Height = 20, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, IsReadOnly = true });
                wrapA.Children.Add(new Label { Content = " W następnej iteracji dodany zostanie bit:", Margin = new Thickness(5, 0, 0, 0) });
                wrapA.Children.Add(new TextBox { Margin = new Thickness(3, 0, 0, 0), FontSize = 14, Text = (sum % 2).ToString(), Width = 20, Height = 20, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, IsReadOnly = true });


                stack.Children.Add(wrapX);
                stack.Children.Add(wrapA);
                stack.Children.Add(new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 1, 0, 0), Margin = new Thickness(15, 2, 15, 2) });
                generatedRegistersStackPanelStepByStep.Children.Add(stack);
            }
        }

        private void t1a_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TextBox)sender).Text = string.Concat(((TextBox)sender).Text.Where(x => x == '0' || x == '1').Select(x => x));
        }

        private void loadFile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void decipherButton_Click(object sender, RoutedEventArgs e)
        {
            if (cipheredText.Text.Length > 0 && keyGlobal.Length > 0)
            {
                String toDecipher = cipheredText.Text;
                cipheredText.Text = "";
                //for (int i = 0; i < toDecipher.Length; i++)
                //{
                //    if (toDecipher[i].Equals(keyGlobal[i]))
                //        cipheredText.Text += "0";
                //    else
                //        cipheredText.Text += "1";
                //}
                var bytes = GetBytesFromBinaryString(toDecipher);
                cipheredText.Text += Encoding.ASCII.GetString(bytes);
            }
            else
                showAlert("Problem z kluczem lub nie ma tekstu do zaszyfrowania");
        }

        private void cipherButton_Click(object sender, RoutedEventArgs e)
        {

            if (textToBeCiphered.Text.Length > 0 && keyGlobal.Length > 0)
            {
                //String toCipher = textToBeCiphered.Text;
                //for(int i = 0; i < toCipher.Length; i++)
                //{
                //    if (toCipher[i].Equals(keyGlobal[i]))
                //        cipheredText.Text += "0";
                //    else
                //        cipheredText.Text += "1";
                //}
                cipheredText.Text = GetBinaryStringFromString(textToBeCiphered.Text);


            }
            else
                showAlert("Problem z kluczem lub nie ma tekstu do zaszyfrowania");
        }

        private void generateKeyBtn_Click(object sender, RoutedEventArgs e)
        {
            if (keyLengthTextBox.Text != "")
            {
                if (int.Parse(keyLengthTextBox.Text) % 8 != 0) { keyLengthTextBox.Text = (int.Parse(keyLengthTextBox.Text) + 8 - int.Parse(keyLengthTextBox.Text) % 8).ToString(); }
                keyTextBox.Text = "";
                counter.Text = "0";
                time.Text = "0m 0s 0ms";
                keyLength = int.Parse(keyLengthTextBox.Text);
                stopKeyBtn.Visibility = Visibility.Visible;
                ThreadPool.QueueUserWorkItem(ThreadProc, keyLengthTextBox);
                generateKeyBtn.IsEnabled = false;
                generateRegisters.IsEnabled = false;
                loadRegisters.IsEnabled = false;

                keySaveButton.Visibility = Visibility.Hidden;
                infoSaveButton.Visibility = Visibility.Hidden;
            }
            else
                showAlert("Nie podano długości klucza!");
        }

        private void ThreadProc(object state)
        {
            DateTime startTime = DateTime.Now;
            keyGlobal = GenerateKey();
            DateTime endTime = DateTime.Now;
            TimeSpan span = endTime.Subtract(startTime);
            keyTextBox.Dispatcher.Invoke(
                 System.Windows.Threading.DispatcherPriority.Normal,
                (ThreadStart)delegate
                {
                    keyTextBox.Text = keyGlobal;
                    counter.Text = keyGlobal.Count().ToString();
                    time.Text = span.Minutes + "m " + span.Seconds + "s " + span.Milliseconds + "ms";
                    generateKeyBtn.IsEnabled = true;
                    generateRegisters.IsEnabled = true;
                    loadRegisters.IsEnabled = true;
                    keySaveButton.Visibility = Visibility.Visible;
                    infoSaveButton.Visibility = Visibility.Visible;
                    stopKeyBtn.Visibility = Visibility.Hidden;
                }
            );
            stop = false;
        }

        #endregion
    }
}
