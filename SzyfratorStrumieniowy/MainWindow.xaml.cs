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
using System.Collections;
using System.Diagnostics;

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
        private List<KeyValuePair<BitArray,BitArray>> registers = new List<KeyValuePair<BitArray, BitArray>>();
        private  List<KeyValuePair<BitArray,BitArray>> registersStepByStep = new  List<KeyValuePair<BitArray,BitArray>>();
        private  List<KeyValuePair<BitArray,BitArray>> registersBackup = new  List<KeyValuePair<BitArray,BitArray>>();
        private List<KeyValuePair<short, BitArray>> perfectPolynomians = new List<KeyValuePair<short, BitArray>>();
        private String helpText = "Autor: Damian Szkudlarek\nRodzaj generatora: Generator progowy\n\nZasada działania generatora progowego opiera się na wspólnej pracy, nieparzystej liczby rejestrów LSFR.\nPrzykład działania:\nZałóżmy że mamy 3 rejestry o różnej długości.W każdej iteracji bity rejestrów przesuwane są o jeden w prawo, tak, że ostatni bit zostaje wypchnięty, a na miejsce pierwszego dostaje się reszta z dzielenia przez dwa wyniku mnożenia rejestru z wielomianem.\nW kolejnym kroku należy zliczyć ile rejestrów wypchnęło bit prawdy. Jeśli suma ta przekracza połowę ilości rejestrów to do klucza dodajemy 1, w przeciwnym przypadku 0.\nWizualizacja przykładu działania generatora znajduje się w zakładce Krok po kroku\n\nWażne!\nGdy długości rejestrów są względnie pierwsze,a wielomiany gałęzi sprzężenia zwrotnego pierwotne, to okres tego generatora jest maksymalny.\nSzum generatora można zauważyć przy długości rejestrów powyżej 10. Wcześniej zauważyć można powatarzający się wzór.\n\n\nFunkcje programu:\n\t-Generowanie rejestrów LSFR,\n\t\t*Ręczne ustawienie parametrów:\n\t\t\t>Liczba rejestrów do wygenerowania,\n\t\t\t>Maksymalna długość pojedynczego rejestru,\n\t\t\t>Wybór pomiędzy losowymi wielomianami, a pierwotnymi*,\n\t-Wizualizacja rejestrów(x) i ich wielomianów(a),\n\t-Wybór długości klucza do wygenerowania,\n\t-Przycisk STOP, przerywający generowanie klucza,\n\t-Pomiar liczby wygenerowanych znaków oraz czasu, w jakim się to stało,\n\t-Zapis wygenerowanego klucza do pliku tekstowego lub binarnego,\n\t-Zapis rejestrów do pliku\n\t-Wczytanie rejestrów z pliku\n\n*Pierwotne wielomiany przedstawione zostały w dodatku w książce Schneier B. Kryptografia dla praktyków\n\n\nUwagi odnośnie programu:\n\t-Generowanie rejestrów nie wykonuje się automatycznie. Po zmianie parametrów należy każdorazowo wcisnąć przycisk Generuj rejestry.\n\t-Program obsługuje tylko pliki tekstowe i binarne.\n\t-Maksymalna długość rejestru została ograniczona w celach prezentacyjnych.\n\t-W programie rejestr i wielomian mają taką samą długość, dodatkowa jedynka przed wielomianem nie wpływa na obliczenia i ma funkcję tylko symboliczną.\n\t-Przy wczytaniu rejestrów z pliku:\n\t\t*Jeśli długość rejestru i wielomianu różni się lub wielomian nie został podany, to wielomian zostaje zastąpiony odpowiadającym długości rejestru, wielomianem pierwotnym.\n\t\t*Jeśli w rejestrze znajdują się same zera, to ostatnie zero zostaje zamienione na jedynkę.\n\t\t*Jeśli w wielomianie najstarszy bit nie jest jedynką to zostaje zamieniony na jedynkę.\n\t\t*Linie pliku, które nie zostały zapisane w odpowiednim formacie zostają pominięte.\n\t\t*Jeśli w pliku zapisano mniej niż 3 poprawne rejestry, lub parzystą liczbę poprawnych rejestrów to wczytywanie zakończy się niepowodzeniem.\n\t\t*Jeśli w pliku dwa rejestry mają taką samą długość to wczytywanie pliku zakończy się niepowodzeniem.\n\nPrzykład obsługi programu - generowanie rejestrów\nKrok 1.\n\tWybierz liczbę rejestrów do wygenerowania.\nKrok 2.\n\tWybierz maksymalną długość rejestru.\nKrok 3.\n\tWybierz rodzaj wielomianu, preferowany Pierwotne.\nKrok 4. \n\tNaciśnij przycisk Generuj rejestry.\n\n\n\n\nPrzykład obsługi programu - generowanie klucza \nKrok 1.\n\tGdy wygenerowano lub wczytano już rejestry, należy wpisać długość klucza.\nKrok 2.\n\tNaciśnij przycisk Generuj klucz.\nKrok 3.\n\tJeśli generowanie klucza trwa za długo, wciśnij przycisk STOP - przerwie to pracę generatora i wyświetli obok część klucza.\nKrok 4. \n\tZapisz klucz do pliku.\nKrok 5.\n\tZapisz rejestry do pliku.\n\n\n\n\nPrzykład obsługi programu - wczytanie rejestów\nKrok 1.\n\tNaciśnij przycisk Wczytaj rejestry.\nKrok 2.\n\tWybierz plik do wczytania. Plik powinien być zapisany w odpowiednim formacie 'rejestr; wielomian' - taki jak przy zapisie rejestrów do pliku.\nKrok 3.\n\tObejrzyj rejestry poniżej.\n";
        private BitArray keyBitArray;
        Stopwatch sw = new Stopwatch();
        private BitArray loadedKeyBit;
        private BitArray toCipherBits;
        private String toCipherChars;
        private String toCipherBitsChars;
        private bool textMode = false;

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
        public static bool GenerateRandom()
        {
            int count = 1, min=0,max=2;
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
            if (result[0] == 1)
                return true;
            else
                return false;
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
            perfectPolynomians.Add(new KeyValuePair<short, BitArray>(3, CreatePolynomian(new List<short>() { 4, 1, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, BitArray>(4, CreatePolynomian(new List<short>() { 5, 2, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, BitArray>(5, CreatePolynomian(new List<short>() { 6, 1, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, BitArray>(6, CreatePolynomian(new List<short>() { 7, 1, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, BitArray>(7, CreatePolynomian(new List<short>() { 8, 4, 3, 2, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, BitArray>(8, CreatePolynomian(new List<short>() { 9, 4, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, BitArray>(9, CreatePolynomian(new List<short>() { 10, 3, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, BitArray>(10, CreatePolynomian(new List<short>() { 11, 2, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, BitArray>(11, CreatePolynomian(new List<short>() { 12, 6, 4, 1, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, BitArray>(12, CreatePolynomian(new List<short>() { 13, 4, 3, 1, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, BitArray>(13, CreatePolynomian(new List<short>() { 14, 5, 3, 1, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, BitArray>(14, CreatePolynomian(new List<short>() { 15, 1, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, BitArray>(15, CreatePolynomian(new List<short>() { 16, 5, 3, 2, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, BitArray>(16, CreatePolynomian(new List<short>() { 17, 3, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, BitArray>(17, CreatePolynomian(new List<short>() { 18, 5, 2, 1, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, BitArray>(18, CreatePolynomian(new List<short>() { 19, 5, 2, 1, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, BitArray>(19, CreatePolynomian(new List<short>() { 20, 3, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, BitArray>(20, CreatePolynomian(new List<short>() { 21, 2, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, BitArray>(21, CreatePolynomian(new List<short>() { 22, 1, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, BitArray>(22, CreatePolynomian(new List<short>() { 23, 5, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, BitArray>(23, CreatePolynomian(new List<short>() { 24, 4, 3, 1, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, BitArray>(24, CreatePolynomian(new List<short>() { 25, 3, 0 })));
            perfectPolynomians.Add(new KeyValuePair<short, BitArray>(25, CreatePolynomian(new List<short>() { 26, 6, 2, 1, 0 })));
        }

        private BitArray CreatePolynomian(List<short> list)
        {
            short length = (short)(list.ElementAt(0) - 1);
            BitArray returnArray = new BitArray(length);
            List<short> dummy = list;
            dummy.RemoveAt(0);
            foreach (short index in dummy)
            {
                short position = (short)(length - (index + 1));
                returnArray.Set(position, true);
            }

            return returnArray;

        }
        #endregion

        #region Utworzenie rejestru
        private KeyValuePair<BitArray,BitArray> CreateRegister()
        {
           
            int randomNumber = -1;
            do
            {
                randomNumber = GenerateRandom(1, 3, maximumLengthOfRegister + 1)[0];
            } while (registersLengths.Contains(randomNumber));
            registersLengths.Add(randomNumber);
            BitArray registerContent = new BitArray(randomNumber);
            BitArray polynomial = new BitArray(randomNumber);

            for (int i = 0; i < randomNumber - 1; i++)
            {
                //polynomial.Add(GenerateRandom(1, 0, 2)[0].ToString());
                polynomial.Set(i, GenerateRandom());
            }
            for (int i = 0; i < randomNumber; i++)
                registerContent.Set(i, GenerateRandom());

            if (!registerContent.Cast<bool>().Any(x => x))
                registerContent[GenerateRandom(1, 0, randomNumber)[0]] = true;
            polynomial[randomNumber-1] = true;
            
            return new KeyValuePair<BitArray,BitArray>(registerContent, polynomial);
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
        private BitArray GetBinaryStringFromString(String data)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in data.ToCharArray())
            {
                sb.Append(Convert.ToString(c, 2).PadLeft(8, '0'));
            }
            return ToBitArray(sb.ToString());
        }
        public BitArray ToBitArray(string str)
        {
            BitArray ret = new BitArray(str.Length);
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '1')
                    ret[i] = true;
                else ret[i] = false;
            }
            return ret;
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

        private List<KeyValuePair<BitArray,BitArray>> CopyList(List<KeyValuePair<BitArray, BitArray>> list)
        {
            List<KeyValuePair<BitArray, BitArray>> temp = new List<KeyValuePair<BitArray, BitArray>>();
            for (int i = 0; i < list.Count(); i++)
                temp.Add(new KeyValuePair<BitArray, BitArray>(list[i].Key, list[i].Value));
            return temp;
        }

        #endregion

        #region Wygenerowanie klucza
        private async void GenerateKey()
        {
            //String key = "";
           
                BitArray key = new BitArray(keyLength);
                registers = CopyList(registersBackup);
                int sum = 0;
                int count = registers.Count;
                int countDivided = count / 2;
            sw.Reset();
            sw.Start();
            await Task.Run(() =>
            {
                for (int i = 0; i < keyLength; i++)
                {
                    sum = 0;
                    for (int j = 0; j < count; j++)
                    {
                        var temp = registers[j];
                        sum += IterateOverRegister(ref temp);
                        registers[j] = temp;
                    }
                    if (sum > countDivided) key.Set(i, true);

                    if (stop == true)
                    {
                        break;
                    }

                }
            });
            keyBitArray = key;
            keyGlobal = ToStringBitArray(key);
            keyTextBox.Text = keyGlobal;
            sw.Stop();

            counter.Text = keyGlobal.Count().ToString();
            time.Text = sw.ElapsedMilliseconds / 60000 + "m " + sw.ElapsedMilliseconds / 1000 + "s " + sw.ElapsedMilliseconds%1000 + "ms";
            generateKeyBtn.IsEnabled = true;
            generateRegisters.IsEnabled = true;
            loadRegisters.IsEnabled = true;
            keySaveButton.Visibility = Visibility.Visible;
            infoSaveButton.Visibility = Visibility.Visible;
            stopKeyBtn.Visibility = Visibility.Hidden;

            stop = false;
        }

        private String ToStringBitArray(BitArray key)
        {
            var builder = new StringBuilder();
            foreach (var bit in key.Cast<bool>())
                builder.Append(bit ? '1' : '0');
            return builder.ToString();
        }
        #endregion

        #region Jedna iteracja po rejestrze
        private int IterateOverRegister(ref KeyValuePair<BitArray, BitArray> register)
        {
            int sum = 0;
            for (int i = 0; i < register.Key.Count; i++)
            {
                if (register.Key.Get(i)==true && register.Value.Get(i) == true)
                    sum += 1;
            }
            int pushedValue = Convert.ToInt32(register.Key.Get(register.Key.Count-1));
            for (int i = register.Key.Count-1; i > 0; i--)
            {
                register.Key[i] = register.Key[i-1];
            }
            register.Key.Set(0, ConvertToBoolean(sum % 2));
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
                        var p2 = perfectPolynomians.Where(z => z.Key == p1.Count).Select(x => x.Value).ToList();
                        registers.RemoveAt(p);
                        registers.Insert(p, new KeyValuePair<BitArray,BitArray>(p1, p2[0]));

                    }
                }
                registersBackup = CopyList(registers);

                fillTheRegistersStackPanel();

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

                foreach (bool xi in register.Key)
                {
                    wrapX.Children.Add(new TextBox { Name = "textboxX" + i.ToString() + j.ToString(), Margin = new Thickness(0.4, 0, 0, 0), FontSize = 9, Text = Convert.ToInt32(xi).ToString(), Width = 13, Height = 13, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, IsReadOnly = true });
                    j++;
                }
                j = 1;
                foreach (bool ai in register.Value)
                {
                    wrapA.Children.Add(new TextBox { Name = "textboxA" + i.ToString() + j.ToString(), Margin = new Thickness(0.4, 0, 0, 0), FontSize = 9, Text = Convert.ToInt32(ai).ToString(), Width = 13, Height = 13, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, IsReadOnly = true });
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
               BitArray aL , xL;
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
                    else { if (!a.Last().Equals('1')) { a.Remove(a.Length - 1, 0); a += '1'; } aL = new BitArray(a.Select(y => Convert.ToBoolean(y)).ToArray()); }
                    xL = new BitArray(x.Select(y => Convert.ToBoolean(y)).ToArray());
                    registers.Add(new KeyValuePair<BitArray,BitArray>(xL, aL));

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

        private bool ConvertToBoolean(char c)
        {
            if (c == '0')
                return false;
            else
                return true;   
        }
        private bool ConvertToBoolean(int i)
        {
            if (i == 0)
                return false;
            else
                return true;
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
                registersStepByStep.Add(new KeyValuePair<BitArray, BitArray>(new BitArray(t1a.Text.Select(x => ConvertToBoolean(x)).ToArray()), perfectPolynomians.Where(x => x.Key == t1a.Text.Count()).Select(x => x.Value).ToList()[0]));
                registersStepByStep.Add(new KeyValuePair<BitArray, BitArray>(new BitArray(t1b.Text.Select(x => ConvertToBoolean(x)).ToArray()), perfectPolynomians.Where(x => x.Key == t1b.Text.Count()).Select(x => x.Value).ToList()[0]));
                registersStepByStep.Add(new KeyValuePair<BitArray, BitArray>(new BitArray(t1c.Text.Select(x => ConvertToBoolean(x)).ToArray()), perfectPolynomians.Where(x => x.Key == t1c.Text.Count()).Select(x => x.Value).ToList()[0]));
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

                foreach (bool ai in register.Value)
                {
                    if (ai == true)
                    {
                        wrapA.Children.Add(new TextBox { Background = Brushes.Yellow, Margin = new Thickness(3, 0, 0, 0), FontSize = 14, Text = Convert.ToInt32(ai).ToString(), Width = 20, Height = 20, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, IsReadOnly = true });
                        turnedOn.Add(j);
                    }
                    else wrapA.Children.Add(new TextBox { Margin = new Thickness(3, 0, 0, 0), FontSize = 14, Text = Convert.ToInt32(ai).ToString(), Width = 20, Height = 20, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, IsReadOnly = true });
                    j++;
                }
                j = 0;
                foreach (bool xi in register.Key)
                {
                    if (turnedOn.Contains(j)) wrapX.Children.Add(new TextBox { Background = Brushes.Yellow, Margin = new Thickness(3, 0, 0, 0), FontSize = 14, Text = Convert.ToInt32(xi).ToString(), Width = 20, Height = 20, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, IsReadOnly = true });
                    else wrapX.Children.Add(new TextBox { Margin = new Thickness(3, 0, 0, 0), FontSize = 14, Text = Convert.ToInt32(xi).ToString(), Width = 20, Height = 20, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, IsReadOnly = true });
                    j++;
                }
                j = 0;
                i++;

                for (int p = 0; p < register.Key.Count; p++)
                {
                    if (register.Key.Get(p) ==true && register.Value.Get(p) == true)
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
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".txt",
                Filter = "Pliki tekstowe (.txt)|*.txt|Pliki binarne (.bin)|*.bin"
            };
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                System.Text.RegularExpressions.Regex _regex = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z0-9\\"" !@#$%^&*()_\-\+=\}\{\:\'\[\]\,\.\\\/\|<>]+$");
                
                var lines = File.ReadAllLines(dlg.FileName, Encoding.GetEncoding("Windows-1250"));
                String fileText = "";
                foreach(var line in lines)
                {
                    if (_regex.IsMatch(line))
                    {
                        fileText += line+Environment.NewLine;
                    }
                    else
                    {
                        showAlert("Niedozowolna treść pliku!");
                        return;
                    }
                }
                textToBeCiphered.Text = fileText;

            }
        }

        

        private BitArray ExtendBitArray(BitArray bitA, int howManyTimes, int howMuchToAdd)
        {
            var bools = new bool[bitA.Count*howManyTimes + howMuchToAdd];
            var bools2 = new bool[bitA.Count];

            for (int i = 0; i < howManyTimes; i++)
            {
                bitA.CopyTo(bools, i*bitA.Count);
            }
            if (howMuchToAdd > 0)
            {
                bitA.CopyTo(bools2, 0);
                for (int i = 0; i < howMuchToAdd; i++)
                {
                    bools[howManyTimes * bitA.Count + i] = bools2[i];
                }
            }
            return new BitArray(bools);
        }

        private void saveTextASCII_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                DefaultExt = ".txt",
                Filter = "Pliki tekstowe (.txt)|*.txt"
            };

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true )
            {
                if (cipheredText.Text != "" && toCipherChars != "")
                {
                    File.WriteAllText(dlg.FileName, toCipherChars, Encoding.GetEncoding("Windows-1250"));

                }
                else
                    showAlert("Nie ma zawartości do zapisania");
            }
        }

        private void saveTextBinary_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                DefaultExt = ".txt",
                Filter = "Pliki tekstowe (.txt)|*.txt"
            };

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                if (cipheredText.Text != "" && toCipherBitsChars!= "")
                {
                    File.WriteAllText(dlg.FileName, toCipherBitsChars, Encoding.GetEncoding("Windows-1250"));

                }
                else
                    showAlert("Nie ma zawartości do zapisania");
            }
        }

        private void saveTextAsBin_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                DefaultExt = ".bin",
                Filter = "Pliki binarne (.bin)|.bin"
            };

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                if (cipheredText.Text != "" && toCipherBitsChars.Count() > 0)
                {
                    int numOfBytes = toCipherBitsChars.Length / 8;
                    byte[] bytes = new byte[numOfBytes];
                    for (int i = 0; i < numOfBytes; ++i)
                    {
                        bytes[i] = Convert.ToByte(toCipherBitsChars.Substring(8 * i, 8), 2);
                    }
                    //toCipherBits.CopyTo(bytes, 0);
                    File.WriteAllBytes(dlg.FileName, bytes);
                }
                else
                    showAlert("Nie ma zawartości do zapisania");
            }
        }

        private void loadKey_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".txt",
                Filter = "Pliki tekstowe (.txt)|*.txt|Pliki binarne (.bin)|*.bin"
            };
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                System.Text.RegularExpressions.Regex _regex = new System.Text.RegularExpressions.Regex("[0-1]+$");
                var key = File.ReadAllText(dlg.FileName, Encoding.GetEncoding("Windows-1250"));
                if (_regex.IsMatch(key))
                {
                    loadedKey.Text = key;
                    loadedKeyBit = ToBitArray(key);
                }
                else
                    showAlert("Niepoprawny klucz.\nKlucz powinien być zapisany binarnie.");
            }
        }
        private void decipherButton_Click(object sender, RoutedEventArgs e)
        {
            loadedKeyBit = ToBitArray(loadedKey.Text);
            if (textToBeCiphered.Text.Length > 0 && loadedKeyBit.Count > 0)
            {
                System.Text.RegularExpressions.Regex _regex = new System.Text.RegularExpressions.Regex("[0-1]+$");
                if (!_regex.IsMatch(loadedKey.Text)) { showAlert("Do deszyfracji należy podać ciąg binarny");return; }
                BitArray newKey;
                if (textToBeCiphered.Text.Length > loadedKeyBit.Count)
                {
                    newKey = ExtendBitArray(loadedKeyBit, (textToBeCiphered.Text.Length / loadedKeyBit.Count), (textToBeCiphered.Text.Length % loadedKeyBit.Count));
                }
                else if (textToBeCiphered.Text.Length == loadedKeyBit.Count)
                {
                    newKey = ExtendBitArray(loadedKeyBit, 1, 0);
                }
                else
                {
                    newKey = ExtendBitArray(loadedKeyBit, 0, textToBeCiphered.Text.Length);
                }
                loadedKeyBit = newKey;
                loadedKey.Text = ToStringBitArray(newKey);
                toCipherBits = ToBitArray(textToBeCiphered.Text);
                BitArray XoredBits = toCipherBits.Xor(newKey);
                toCipherChars = Encoding.ASCII.GetString(GetBytesFromBinaryString(ToStringBitArray(XoredBits)));
                toCipherBitsChars = ToStringBitArray(XoredBits);
                if (textMode)
                {
                    cipheredText.Text = toCipherChars;
                }
                else
                    cipheredText.Text = toCipherBitsChars;
            }
            else
                showAlert("Problem z kluczem lub nie ma tekstu do zaszyfrowania");
        }

        private void changeView_Click(object sender, RoutedEventArgs e)
        {
            if (textMode)
            {
                textMode = false;
                cipheredText.Text = toCipherBitsChars;
            }
            else
            {
                textMode = true;
                cipheredText.Text = toCipherChars;
            }
        }

        private void loadedKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Text.RegularExpressions.Regex _regex = new System.Text.RegularExpressions.Regex("[0-1]+$");
            loadedKey.Text = string.Concat(loadedKey.Text.Where(x => _regex.IsMatch(x.ToString())).Select(x => x));
        }

        private void cipherButton_Click(object sender, RoutedEventArgs e)
        {
            loadedKeyBit = ToBitArray(loadedKey.Text);
            if (textToBeCiphered.Text.Length > 0 && loadedKeyBit.Count > 0)
            {
                var lines = textToBeCiphered.Text.Split(new[] { "\r\n", "\r", "\n" },StringSplitOptions.None);
                System.Text.RegularExpressions.Regex _regex = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z0-9\\"" !@#$%^&*()_\-\+=\}\{\:\'\[\]\,\.\\\/\|<>]*$");
                foreach (var line in lines)
                {
                    if (!_regex.IsMatch(line)) { showAlert("Niedozowolna treść w polu Tekst jawny!"); return; }
                }
                toCipherBits = GetBinaryStringFromString(textToBeCiphered.Text);
                BitArray newKey;
                if(toCipherBits.Count != loadedKeyBit.Count) {
                    MessageBoxResult result = MessageBox.Show("Klucz i tekst jawny mają różną długość.\nCzy zgadzasz się na automatyczną poprawę? (powielenie klucza lub skrócenie go)", "Uwaga", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.No) return;
                }
                if (toCipherBits.Count > loadedKeyBit.Count)
                {
                    newKey = ExtendBitArray(loadedKeyBit, (toCipherBits.Count / loadedKeyBit.Count), (toCipherBits.Count % loadedKeyBit.Count));
                }
                else if(toCipherBits.Count == loadedKeyBit.Count) {
                    newKey = ExtendBitArray(loadedKeyBit, 1,0);
                }
                else
                {
                    newKey = ExtendBitArray(loadedKeyBit, 0, toCipherBits.Count);
                }
                loadedKeyBit = newKey;
                loadedKey.Text = ToStringBitArray(newKey);
                BitArray XoredBits = toCipherBits.Xor(newKey);
                toCipherChars = Encoding.ASCII.GetString(GetBytesFromBinaryString(ToStringBitArray(XoredBits)));
                toCipherBitsChars = ToStringBitArray(XoredBits);
                if (textMode)
                {
                    cipheredText.Text = toCipherChars;
                }
                else
                    cipheredText.Text = toCipherBitsChars;
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
                GenerateKey();
                generateKeyBtn.IsEnabled = false;
                generateRegisters.IsEnabled = false;
                loadRegisters.IsEnabled = false;

                keySaveButton.Visibility = Visibility.Hidden;
                infoSaveButton.Visibility = Visibility.Hidden;
            }
            else
                showAlert("Nie podano długości klucza!");
        }

        #endregion
    }
}
