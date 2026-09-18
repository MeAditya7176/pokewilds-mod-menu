using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Windows.Forms;

[assembly: AssemblyTitle("PokeWilds Mod Menu and Spawner")]
[assembly: AssemblyDescription("Mod Menu, Pokemon Spawner and Save Editor for PokeWilds")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("PokeWilds Community")]
[assembly: AssemblyProduct("PokeWilds Mod Menu")]
[assembly: AssemblyCopyright("Copyright (C) 2026 PokeWilds Community")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: System.Runtime.InteropServices.ComVisible(false)]
[assembly: System.Runtime.InteropServices.Guid("e2b5c7a1-8f34-4d92-b106-7e5c9a124d3f")]
[assembly: AssemblyVersion("1.5.0.0")]
[assembly: AssemblyFileVersion("1.5.0.0")]

namespace PokeWildsModMenu
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new ModMenuForm());
            }
            catch (Exception ex)
            {
                File.WriteAllText(@"modmenu_error.log", ex.ToString());
                MessageBox.Show(ex.Message, "PokéWilds Mod Menu Error");
            }
        }
    }

    public class ModMenuForm : Form
    {
        private string gameDir = @"F:\pokewilds-v0.8.11-windows-64";
        private string activeSavFolder = "";
        private string activeZipPath = "";
        private Dictionary<string, object> saveData = null;
        private JavaScriptSerializer jsonSer = new JavaScriptSerializer();

        // Top Controls
        private ComboBox cmbSaves;
        private Label lblPlayerInfo;
        private Label lblStatus;
        private TabControl tabControl;

        // Custom Top Nav Buttons
        private Button btnNavItems;
        private Button btnNavPokemon;
        private Button btnNavCheats;
        private Button btnNavWorld;

        // Items Tab
        private ListView listItems;
        private ComboBox cmbItemSelect;
        private NumericUpDown numItemQty;

        // Pokemon Tab
        private ListBox listParty;
        private TextBox txtPokeNick;
        private NumericUpDown numPokeLevel;
        private NumericUpDown numPokeHp;
        private CheckBox chkPokeShiny;
        private NumericUpDown numPokeFriend;
        private TextBox txtMove1;
        private TextBox txtMove2;
        private TextBox txtMove3;
        private TextBox txtMove4;

        // Pokemon Spawner Controls
        private ComboBox cmbSpawnPoke;
        private NumericUpDown numSpawnLevel;
        private CheckBox chkSpawnShiny;
        private ComboBox cmbSpawnGender;
        private TextBox txtSpawnNick;

        // Cheat Console Tab
        private TextBox txtCheatCode;
        private ListBox listCheatLog;

        // World Tab
        private RadioButton radDay;
        private RadioButton radNight;
        private RadioButton radDusk;

        private readonly string[] allItems = new string[] {
            "master ball", "ultra ball", "great ball", "poke ball", "dusk ball", "heavy ball", 
            "moon ball", "love ball", "fast ball", "level ball", "lure ball", "friend ball", 
            "net ball", "dive ball", "quick ball", "heal ball", "nest ball", "timer ball", "dream ball",
            "rare candy", "ragecandybar", "berry juice", "escape rope", "sleeping bag",
            "fire stone", "water stone", "thunderstone", "leaf stone", "moon stone", "sun stone", 
            "dawn stone", "dusk stone", "shiny stone", "ice stone", "hard stone", "oval stone", "everstone",
            "lum berry", "pecha berry", "cheri berry", "aspear berry", "chesto berry", "rawst berry", "persim berry",
            "wood", "soft soil", "brick", "charcoal", "fertilizer", "silk", "metal coat", "dragon scale", "kings rock", "life force"
        };

        private List<string> allPokemonNames = new List<string>();

        public ModMenuForm()
        {
            this.Text = "PokéWilds Mod Menu & Spawner v1.5";
            this.Size = new Size(910, 715);
            this.MinimumSize = new Size(910, 715);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(24, 26, 32);
            this.ForeColor = Color.FromArgb(240, 240, 245);
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            // Auto-detect game directory
            string appDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\', '/');
            if (Directory.Exists(Path.Combine(appDir, "app")) || File.Exists(Path.Combine(appDir, "pokewilds.exe")))
            {
                gameDir = appDir;
            }

            LoadPokemonNames();
            InitUI();
            ScanSaves();
        }

        private void LoadPokemonNames()
        {
            string pFile = Path.Combine(gameDir, "pokemon_list.txt");
            if (!File.Exists(pFile)) pFile = @"C:\Users\Admin\.gemini\antigravity-ide\scratch\all_pokemon.txt";
            if (File.Exists(pFile))
            {
                string[] lines = File.ReadAllLines(pFile);
                foreach (string l in lines)
                {
                    string t = l.Trim().ToLower();
                    if (!string.IsNullOrEmpty(t) && !allPokemonNames.Contains(t))
                    {
                        allPokemonNames.Add(t);
                    }
                }
            }
            if (allPokemonNames.Count == 0)
            {
                allPokemonNames.AddRange(new string[] { "charizard", "mewtwo", "pikachu", "lucario", "gengar", "rayquaza", "tyranitar", "dragonite", "blastoise", "machop" });
            }
        }

        private void SwitchTab(int index)
        {
            tabControl.SelectedIndex = index;
            Color activeColor = Color.FromArgb(99, 102, 241);
            Color idleColor = Color.FromArgb(45, 49, 60);

            btnNavItems.BackColor = (index == 0) ? activeColor : idleColor;
            btnNavPokemon.BackColor = (index == 1) ? activeColor : idleColor;
            btnNavCheats.BackColor = (index == 2) ? activeColor : idleColor;
            btnNavWorld.BackColor = (index == 3) ? activeColor : idleColor;
        }

        private void InitUI()
        {
            // Top Header Panel
            Panel topPanel = new Panel();
            topPanel.Dock = DockStyle.Top;
            topPanel.Height = 118;
            topPanel.BackColor = Color.FromArgb(32, 35, 44);
            this.Controls.Add(topPanel);

            Label lblTitle = new Label();
            lblTitle.Text = "⚡ PokéWilds Mod Menu & Spawner";
            lblTitle.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(129, 140, 248);
            lblTitle.Location = new Point(16, 12);
            lblTitle.AutoSize = true;
            topPanel.Controls.Add(lblTitle);

            Label lblSelectWorld = new Label();
            lblSelectWorld.Text = "Active World:";
            lblSelectWorld.Location = new Point(510, 16);
            lblSelectWorld.AutoSize = true;
            topPanel.Controls.Add(lblSelectWorld);

            cmbSaves = new ComboBox();
            cmbSaves.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSaves.Location = new Point(595, 13);
            cmbSaves.Width = 165;
            cmbSaves.BackColor = Color.FromArgb(45, 49, 60);
            cmbSaves.ForeColor = Color.White;
            cmbSaves.SelectedIndexChanged += delegate { LoadSelectedSave(); };
            topPanel.Controls.Add(cmbSaves);

            Button btnRefresh = CreateStyledButton("🔄 Refresh", 770, 11, 95, 28, Color.FromArgb(59, 130, 246));
            btnRefresh.Click += delegate { ScanSaves(); };
            topPanel.Controls.Add(btnRefresh);

            lblPlayerInfo = new Label();
            lblPlayerInfo.Text = "Player: Loading... | No Save Loaded";
            lblPlayerInfo.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
            lblPlayerInfo.ForeColor = Color.FromArgb(156, 163, 175);
            lblPlayerInfo.Location = new Point(18, 42);
            lblPlayerInfo.AutoSize = true;
            topPanel.Controls.Add(lblPlayerInfo);

            // Row 3: Navigation Tab Buttons
            btnNavItems = CreateStyledButton("🎒 Items & Bag", 16, 70, 175, 36, Color.FromArgb(99, 102, 241));
            btnNavItems.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            btnNavItems.Click += delegate { SwitchTab(0); };
            topPanel.Controls.Add(btnNavItems);

            btnNavPokemon = CreateStyledButton("⭐ Pokémon Spawner", 200, 70, 225, 36, Color.FromArgb(45, 49, 60));
            btnNavPokemon.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            btnNavPokemon.Click += delegate { SwitchTab(1); };
            topPanel.Controls.Add(btnNavPokemon);

            btnNavCheats = CreateStyledButton("📜 GBA Cheat Codes", 435, 70, 195, 36, Color.FromArgb(45, 49, 60));
            btnNavCheats.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            btnNavCheats.Click += delegate { SwitchTab(2); };
            topPanel.Controls.Add(btnNavCheats);

            btnNavWorld = CreateStyledButton("🌍 World & Time", 640, 70, 160, 36, Color.FromArgb(45, 49, 60));
            btnNavWorld.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            btnNavWorld.Click += delegate { SwitchTab(3); };
            topPanel.Controls.Add(btnNavWorld);

            // Bottom Status & Action Panel
            Panel bottomPanel = new Panel();
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Height = 65;
            bottomPanel.BackColor = Color.FromArgb(32, 35, 44);
            this.Controls.Add(bottomPanel);

            lblStatus = new Label();
            lblStatus.Text = "Ready to modify PokéWilds saves!";
            lblStatus.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
            lblStatus.ForeColor = Color.FromArgb(156, 163, 175);
            lblStatus.Location = new Point(16, 10);
            lblStatus.AutoSize = true;
            bottomPanel.Controls.Add(lblStatus);

            Label lblReloadTip = new Label();
            lblReloadTip.Text = "💡 Tip: After saving in Mod Menu, press Save in-game -> Title -> Continue to load changes!";
            lblReloadTip.Font = new Font("Segoe UI", 8.5f, FontStyle.Italic);
            lblReloadTip.ForeColor = Color.FromArgb(251, 191, 36);
            lblReloadTip.Location = new Point(16, 32);
            lblReloadTip.AutoSize = true;
            bottomPanel.Controls.Add(lblReloadTip);

            Button btnSaveAll = CreateStyledButton("💾 Apply & Save to Game", 620, 12, 255, 40, Color.FromArgb(16, 185, 129));
            btnSaveAll.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            btnSaveAll.Click += delegate { ApplyAndSave("All Changes Saved Successfully!"); };
            bottomPanel.Controls.Add(btnSaveAll);

            // Tab Control
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.Appearance = TabAppearance.FlatButtons;
            tabControl.ItemSize = new Size(0, 1);
            tabControl.SizeMode = TabSizeMode.Fixed;
            this.Controls.Add(tabControl);

            topPanel.SendToBack();
            bottomPanel.SendToBack();
            tabControl.BringToFront();

            InitItemsTab();
            InitPokemonTab();
            InitCheatCodesTab();
            InitWorldTab();
        }

        // ----------------------------------------------------
        // TAB 1: ITEMS & INVENTORY
        // ----------------------------------------------------
        private void InitItemsTab()
        {
            TabPage tab = new TabPage("Items");
            tab.BackColor = Color.FromArgb(24, 26, 32);
            tab.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            tabControl.TabPages.Add(tab);

            // Left Quick Cheats
            GroupBox grpQuick = new GroupBox();
            grpQuick.Text = "⚡ Quick 1-Click Item Cheats";
            grpQuick.ForeColor = Color.FromArgb(129, 140, 248);
            grpQuick.Location = new Point(16, 10);
            grpQuick.Size = new Size(260, 450);
            tab.Controls.Add(grpQuick);

            Button btnMasterBalls = CreateStyledButton("⭐ +99 Master Balls", 20, 30, 220, 36, Color.FromArgb(124, 58, 237));
            btnMasterBalls.Click += delegate { AddItemDirectly("master ball", 99); };
            grpQuick.Controls.Add(btnMasterBalls);

            Button btnRareCandies = CreateStyledButton("🍬 +99 Rare Candies", 20, 75, 220, 36, Color.FromArgb(236, 72, 153));
            btnRareCandies.Click += delegate { AddItemDirectly("rare candy", 99); };
            grpQuick.Controls.Add(btnRareCandies);

            Button btnAllStones = CreateStyledButton("💎 +20 All Evo Stones", 20, 120, 220, 36, Color.FromArgb(14, 165, 233));
            btnAllStones.Click += delegate { AddAllEvoStones(20); };
            grpQuick.Controls.Add(btnAllStones);

            Button btnBerriesRopes = CreateStyledButton("🌿 +50 Berries & Ropes", 20, 165, 220, 36, Color.FromArgb(34, 197, 94));
            btnBerriesRopes.Click += delegate { AddBerriesAndRopes(50); };
            grpQuick.Controls.Add(btnBerriesRopes);

            Button btnAllBalls = CreateStyledButton("🎯 +50 All Special Poké Balls", 20, 210, 220, 36, Color.FromArgb(245, 158, 11));
            btnAllBalls.Click += delegate { AddAllPokeballs(50); };
            grpQuick.Controls.Add(btnAllBalls);

            Button btnMaxExisting = CreateStyledButton("📦 Max All Items to 999", 20, 255, 220, 36, Color.FromArgb(239, 68, 68));
            btnMaxExisting.Click += delegate { MaxOutAllItems(); };
            grpQuick.Controls.Add(btnMaxExisting);

            // Center & Right: Custom Item Adder & Bag List
            GroupBox grpCustom = new GroupBox();
            grpCustom.Text = "➕ Custom Item Adder";
            grpCustom.ForeColor = Color.FromArgb(129, 140, 248);
            grpCustom.Location = new Point(295, 10);
            grpCustom.Size = new Size(575, 95);
            tab.Controls.Add(grpCustom);

            Label lblItem = new Label();
            lblItem.Text = "Select Item:";
            lblItem.Location = new Point(15, 22);
            lblItem.AutoSize = true;
            grpCustom.Controls.Add(lblItem);

            cmbItemSelect = new ComboBox();
            cmbItemSelect.Location = new Point(15, 48);
            cmbItemSelect.Width = 230;
            cmbItemSelect.BackColor = Color.FromArgb(45, 49, 60);
            cmbItemSelect.ForeColor = Color.White;
            foreach (string itm in allItems) cmbItemSelect.Items.Add(itm);
            cmbItemSelect.SelectedIndex = 0;
            grpCustom.Controls.Add(cmbItemSelect);

            Label lblQty = new Label();
            lblQty.Text = "Quantity:";
            lblQty.Location = new Point(260, 22);
            lblQty.AutoSize = true;
            grpCustom.Controls.Add(lblQty);

            numItemQty = new NumericUpDown();
            numItemQty.Location = new Point(260, 48);
            numItemQty.Width = 85;
            numItemQty.Minimum = 1;
            numItemQty.Maximum = 9999;
            numItemQty.Value = 99;
            numItemQty.BackColor = Color.FromArgb(45, 49, 60);
            numItemQty.ForeColor = Color.White;
            grpCustom.Controls.Add(numItemQty);

            Button btnAddCustom = CreateStyledButton("➕ Add to Bag", 365, 45, 190, 32, Color.FromArgb(16, 185, 129));
            btnAddCustom.Click += delegate {
                string itm = cmbItemSelect.Text.Trim().ToLower();
                if (!string.IsNullOrEmpty(itm)) {
                    AddItemDirectly(itm, (int)numItemQty.Value);
                }
            };
            grpCustom.Controls.Add(btnAddCustom);

            // Bag Items ListView
            GroupBox grpBag = new GroupBox();
            grpBag.Text = "🎒 Current Bag Contents";
            grpBag.ForeColor = Color.FromArgb(129, 140, 248);
            grpBag.Location = new Point(295, 115);
            grpBag.Size = new Size(575, 345);
            tab.Controls.Add(grpBag);

            listItems = new ListView();
            listItems.Location = new Point(15, 25);
            listItems.Size = new Size(420, 305);
            listItems.View = View.Details;
            listItems.FullRowSelect = true;
            listItems.BackColor = Color.FromArgb(32, 35, 44);
            listItems.ForeColor = Color.White;
            listItems.Columns.Add("Item Name", 260);
            listItems.Columns.Add("Quantity", 130);
            grpBag.Controls.Add(listItems);

            Button btnRemoveItem = CreateStyledButton("❌ Remove", 450, 30, 110, 32, Color.FromArgb(239, 68, 68));
            btnRemoveItem.Click += delegate { RemoveSelectedItem(); };
            grpBag.Controls.Add(btnRemoveItem);

            Button btnPlus10 = CreateStyledButton("+10 Qty", 450, 75, 110, 30, Color.FromArgb(59, 130, 246));
            btnPlus10.Click += delegate { AdjustSelectedItem(10); };
            grpBag.Controls.Add(btnPlus10);

            Button btnSet99 = CreateStyledButton("Set 99", 450, 115, 110, 30, Color.FromArgb(16, 185, 129));
            btnSet99.Click += delegate { SetSelectedItemQty(99); };
            grpBag.Controls.Add(btnSet99);
        }

        // ----------------------------------------------------
        // TAB 2: POKEMON PARTY & SPAWNER
        // ----------------------------------------------------
        private void InitPokemonTab()
        {
            TabPage tab = new TabPage("Pokemon");
            tab.BackColor = Color.FromArgb(24, 26, 32);
            tab.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            tabControl.TabPages.Add(tab);

            // Left Column: Party List
            GroupBox grpParty = new GroupBox();
            grpParty.Text = "Party (Max 6)";
            grpParty.ForeColor = Color.FromArgb(129, 140, 248);
            grpParty.Location = new Point(12, 10);
            grpParty.Size = new Size(265, 455);
            tab.Controls.Add(grpParty);

            listParty = new ListBox();
            listParty.Location = new Point(12, 25);
            listParty.Size = new Size(240, 270);
            listParty.BackColor = Color.FromArgb(32, 35, 44);
            listParty.ForeColor = Color.White;
            listParty.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            listParty.SelectedIndexChanged += delegate { DisplaySelectedPokemon(); };
            grpParty.Controls.Add(listParty);

            Button btnPartyShiny = CreateStyledButton("✨ Make All Shiny", 12, 305, 240, 32, Color.FromArgb(245, 158, 11));
            btnPartyShiny.Click += delegate { MakeEntirePartyShiny(); };
            grpParty.Controls.Add(btnPartyShiny);

            Button btnPartyLvl100 = CreateStyledButton("⚡ Level 100 All", 12, 342, 240, 32, Color.FromArgb(16, 185, 129));
            btnPartyLvl100.Click += delegate { Level100EntireParty(); };
            grpParty.Controls.Add(btnPartyLvl100);

            Button btnReleasePoke = CreateStyledButton("❌ Release / Delete Selected", 12, 380, 240, 32, Color.FromArgb(239, 68, 68));
            btnReleasePoke.Click += delegate { ReleaseSelectedPokemon(); };
            grpParty.Controls.Add(btnReleasePoke);

            // Middle Column: Edit Selected Pokemon
            GroupBox grpDetails = new GroupBox();
            grpDetails.Text = "Edit Selected Pokémon";
            grpDetails.ForeColor = Color.FromArgb(129, 140, 248);
            grpDetails.Location = new Point(285, 10);
            grpDetails.Size = new Size(280, 455);
            tab.Controls.Add(grpDetails);

            Label lblNick = new Label();
            lblNick.Text = "Nickname:";
            lblNick.Location = new Point(10, 25);
            lblNick.AutoSize = true;
            grpDetails.Controls.Add(lblNick);

            txtPokeNick = new TextBox();
            txtPokeNick.Location = new Point(85, 22);
            txtPokeNick.Width = 180;
            txtPokeNick.BackColor = Color.FromArgb(45, 49, 60);
            txtPokeNick.ForeColor = Color.White;
            grpDetails.Controls.Add(txtPokeNick);

            Label lblLvl = new Label();
            lblLvl.Text = "Level:";
            lblLvl.Location = new Point(10, 60);
            lblLvl.AutoSize = true;
            grpDetails.Controls.Add(lblLvl);

            numPokeLevel = new NumericUpDown();
            numPokeLevel.Location = new Point(85, 58);
            numPokeLevel.Width = 60;
            numPokeLevel.Minimum = 1;
            numPokeLevel.Maximum = 100;
            numPokeLevel.BackColor = Color.FromArgb(45, 49, 60);
            numPokeLevel.ForeColor = Color.White;
            grpDetails.Controls.Add(numPokeLevel);

            Button btnLvl100 = CreateStyledButton("Lvl 100", 155, 56, 110, 26, Color.FromArgb(16, 185, 129));
            btnLvl100.Click += delegate { numPokeLevel.Value = 100; };
            grpDetails.Controls.Add(btnLvl100);

            Label lblHp = new Label();
            lblHp.Text = "HP:";
            lblHp.Location = new Point(10, 95);
            lblHp.AutoSize = true;
            grpDetails.Controls.Add(lblHp);

            numPokeHp = new NumericUpDown();
            numPokeHp.Location = new Point(85, 93);
            numPokeHp.Width = 60;
            numPokeHp.Minimum = 1;
            numPokeHp.Maximum = 9999;
            numPokeHp.BackColor = Color.FromArgb(45, 49, 60);
            numPokeHp.ForeColor = Color.White;
            grpDetails.Controls.Add(numPokeHp);

            Button btnHeal = CreateStyledButton("💖 Heal (350)", 155, 91, 110, 26, Color.FromArgb(239, 68, 68));
            btnHeal.Click += delegate { numPokeHp.Value = 350; };
            grpDetails.Controls.Add(btnHeal);

            Label lblFriend = new Label();
            lblFriend.Text = "Friend:";
            lblFriend.Location = new Point(10, 130);
            lblFriend.AutoSize = true;
            grpDetails.Controls.Add(lblFriend);

            numPokeFriend = new NumericUpDown();
            numPokeFriend.Location = new Point(85, 128);
            numPokeFriend.Width = 60;
            numPokeFriend.Minimum = 0;
            numPokeFriend.Maximum = 255;
            numPokeFriend.BackColor = Color.FromArgb(45, 49, 60);
            numPokeFriend.ForeColor = Color.White;
            grpDetails.Controls.Add(numPokeFriend);

            Button btnMaxFriend = CreateStyledButton("Max (255)", 155, 126, 110, 26, Color.FromArgb(236, 72, 153));
            btnMaxFriend.Click += delegate { numPokeFriend.Value = 255; };
            grpDetails.Controls.Add(btnMaxFriend);

            chkPokeShiny = new CheckBox();
            chkPokeShiny.Text = "✨ Is Shiny Pokémon";
            chkPokeShiny.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            chkPokeShiny.ForeColor = Color.FromArgb(251, 191, 36);
            chkPokeShiny.Location = new Point(10, 160);
            chkPokeShiny.AutoSize = true;
            grpDetails.Controls.Add(chkPokeShiny);

            Label lblMoves = new Label();
            lblMoves.Text = "Attacks (4 Moves):";
            lblMoves.Location = new Point(10, 190);
            lblMoves.AutoSize = true;
            grpDetails.Controls.Add(lblMoves);

            txtMove1 = new TextBox(); txtMove1.Location = new Point(10, 215); txtMove1.Width = 125; txtMove1.BackColor = Color.FromArgb(45, 49, 60); txtMove1.ForeColor = Color.White; grpDetails.Controls.Add(txtMove1);
            txtMove2 = new TextBox(); txtMove2.Location = new Point(145, 215); txtMove2.Width = 125; txtMove2.BackColor = Color.FromArgb(45, 49, 60); txtMove2.ForeColor = Color.White; grpDetails.Controls.Add(txtMove2);
            txtMove3 = new TextBox(); txtMove3.Location = new Point(10, 245); txtMove3.Width = 125; txtMove3.BackColor = Color.FromArgb(45, 49, 60); txtMove3.ForeColor = Color.White; grpDetails.Controls.Add(txtMove3);
            txtMove4 = new TextBox(); txtMove4.Location = new Point(145, 245); txtMove4.Width = 125; txtMove4.BackColor = Color.FromArgb(45, 49, 60); txtMove4.ForeColor = Color.White; grpDetails.Controls.Add(txtMove4);

            Button btnGodMoves = CreateStyledButton("🔥 God Moveset", 10, 280, 260, 28, Color.FromArgb(245, 158, 11));
            btnGodMoves.Click += delegate {
                txtMove1.Text = "flamethrower";
                txtMove2.Text = "thunderbolt";
                txtMove3.Text = "earthquake";
                txtMove4.Text = "psychic";
            };
            grpDetails.Controls.Add(btnGodMoves);

            Button btnSavePoke = CreateStyledButton("💾 Save Changes", 10, 318, 260, 36, Color.FromArgb(16, 185, 129));
            btnSavePoke.Click += delegate { SaveSelectedPokemonDetails(); };
            grpDetails.Controls.Add(btnSavePoke);

            // Right Column: Spawn & Add Pokemon
            GroupBox grpSpawn = new GroupBox();
            grpSpawn.Text = "➕ Spawn / Add Pokémon (900+ Available)";
            grpSpawn.ForeColor = Color.FromArgb(52, 211, 153);
            grpSpawn.Location = new Point(575, 10);
            grpSpawn.Size = new Size(300, 455);
            tab.Controls.Add(grpSpawn);

            Label lblChoose = new Label();
            lblChoose.Text = "Search Pokémon Name:";
            lblChoose.Location = new Point(12, 22);
            lblChoose.AutoSize = true;
            grpSpawn.Controls.Add(lblChoose);

            cmbSpawnPoke = new ComboBox();
            cmbSpawnPoke.Location = new Point(12, 45);
            cmbSpawnPoke.Width = 270;
            cmbSpawnPoke.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbSpawnPoke.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbSpawnPoke.BackColor = Color.FromArgb(45, 49, 60);
            cmbSpawnPoke.ForeColor = Color.White;
            foreach (string pName in allPokemonNames) cmbSpawnPoke.Items.Add(pName);
            if (cmbSpawnPoke.Items.Count > 0) cmbSpawnPoke.SelectedIndex = 0;
            grpSpawn.Controls.Add(cmbSpawnPoke);

            Label lblSpawnLvl = new Label();
            lblSpawnLvl.Text = "Spawn Level:";
            lblSpawnLvl.Location = new Point(12, 80);
            lblSpawnLvl.AutoSize = true;
            grpSpawn.Controls.Add(lblSpawnLvl);

            numSpawnLevel = new NumericUpDown();
            numSpawnLevel.Location = new Point(105, 78);
            numSpawnLevel.Width = 65;
            numSpawnLevel.Minimum = 1;
            numSpawnLevel.Maximum = 100;
            numSpawnLevel.Value = 50;
            numSpawnLevel.BackColor = Color.FromArgb(45, 49, 60);
            numSpawnLevel.ForeColor = Color.White;
            grpSpawn.Controls.Add(numSpawnLevel);

            Button btnSpawnLvl100 = CreateStyledButton("Max 100", 180, 76, 100, 26, Color.FromArgb(16, 185, 129));
            btnSpawnLvl100.Click += delegate { numSpawnLevel.Value = 100; };
            grpSpawn.Controls.Add(btnSpawnLvl100);

            Label lblGender = new Label();
            lblGender.Text = "Gender:";
            lblGender.Location = new Point(12, 115);
            lblGender.AutoSize = true;
            grpSpawn.Controls.Add(lblGender);

            cmbSpawnGender = new ComboBox();
            cmbSpawnGender.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSpawnGender.Location = new Point(105, 112);
            cmbSpawnGender.Width = 175;
            cmbSpawnGender.BackColor = Color.FromArgb(45, 49, 60);
            cmbSpawnGender.ForeColor = Color.White;
            cmbSpawnGender.Items.Add("male");
            cmbSpawnGender.Items.Add("female");
            cmbSpawnGender.Items.Add("genderless");
            cmbSpawnGender.SelectedIndex = 0;
            grpSpawn.Controls.Add(cmbSpawnGender);

            Label lblSpawnNick = new Label();
            lblSpawnNick.Text = "Nickname:";
            lblSpawnNick.Location = new Point(12, 150);
            lblSpawnNick.AutoSize = true;
            grpSpawn.Controls.Add(lblSpawnNick);

            txtSpawnNick = new TextBox();
            txtSpawnNick.Location = new Point(105, 147);
            txtSpawnNick.Width = 175;
            txtSpawnNick.BackColor = Color.FromArgb(45, 49, 60);
            txtSpawnNick.ForeColor = Color.White;
            grpSpawn.Controls.Add(txtSpawnNick);

            chkSpawnShiny = new CheckBox();
            chkSpawnShiny.Text = "✨ Spawn as Shiny Pokémon!";
            chkSpawnShiny.Checked = true;
            chkSpawnShiny.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            chkSpawnShiny.ForeColor = Color.FromArgb(251, 191, 36);
            chkSpawnShiny.Location = new Point(12, 180);
            chkSpawnShiny.AutoSize = true;
            grpSpawn.Controls.Add(chkSpawnShiny);

            Button btnAddPokemon = CreateStyledButton("➕ Add to Party (Slot)", 12, 212, 270, 42, Color.FromArgb(16, 185, 129));
            btnAddPokemon.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            btnAddPokemon.Click += delegate {
                string pName = cmbSpawnPoke.Text.Trim().ToLower();
                if (!string.IsNullOrEmpty(pName))
                {
                    AddNewPokemonToParty(pName, (int)numSpawnLevel.Value, chkSpawnShiny.Checked, cmbSpawnGender.Text, txtSpawnNick.Text.Trim());
                }
            };
            grpSpawn.Controls.Add(btnAddPokemon);

            Label lblPresets = new Label();
            lblPresets.Text = "⚡ Quick Preset Spawners:";
            lblPresets.Location = new Point(12, 265);
            lblPresets.AutoSize = true;
            grpSpawn.Controls.Add(lblPresets);

            Button btnMewtwo = CreateStyledButton("🔮 Shiny Mewtwo (Lvl 100)", 12, 290, 270, 30, Color.FromArgb(124, 58, 237));
            btnMewtwo.Click += delegate { AddNewPokemonToParty("mewtwo", 100, true, "genderless", "Mewtwo"); };
            grpSpawn.Controls.Add(btnMewtwo);

            Button btnCharizard = CreateStyledButton("🔥 Shiny Charizard (Lvl 100)", 12, 325, 270, 30, Color.FromArgb(239, 68, 68));
            btnCharizard.Click += delegate { AddNewPokemonToParty("charizard", 100, true, "male", "Charizard"); };
            grpSpawn.Controls.Add(btnCharizard);

            Button btnRayquaza = CreateStyledButton("🐉 Shiny Rayquaza (Lvl 100)", 12, 360, 270, 30, Color.FromArgb(34, 197, 94));
            btnRayquaza.Click += delegate { AddNewPokemonToParty("rayquaza", 100, true, "genderless", "Rayquaza"); };
            grpSpawn.Controls.Add(btnRayquaza);

            Button btnGengar = CreateStyledButton("👻 Shiny Gengar (Lvl 100)", 12, 395, 270, 30, Color.FromArgb(147, 51, 234));
            btnGengar.Click += delegate { AddNewPokemonToParty("gengar", 100, true, "male", "Gengar"); };
            grpSpawn.Controls.Add(btnGengar);
        }

        // ----------------------------------------------------
        // TAB 3: GBA CHEAT CODES CONSOLE
        // ----------------------------------------------------
        private void InitCheatCodesTab()
        {
            TabPage tab = new TabPage("Cheats");
            tab.BackColor = Color.FromArgb(24, 26, 32);
            tab.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            tabControl.TabPages.Add(tab);

            Label lblInfo = new Label();
            lblInfo.Text = "Enter classic GameShark / Action Replay style cheat codes below:";
            lblInfo.Location = new Point(20, 20);
            lblInfo.AutoSize = true;
            tab.Controls.Add(lblInfo);

            txtCheatCode = new TextBox();
            txtCheatCode.Location = new Point(20, 50);
            txtCheatCode.Width = 570;
            txtCheatCode.Font = new Font("Consolas", 12f, FontStyle.Bold);
            txtCheatCode.BackColor = Color.FromArgb(32, 35, 44);
            txtCheatCode.ForeColor = Color.FromArgb(52, 211, 153);
            txtCheatCode.KeyDown += delegate(object s, KeyEventArgs e) {
                if (e.KeyCode == Keys.Enter) {
                    ExecuteCheatCode(txtCheatCode.Text);
                    e.SuppressKeyPress = true;
                }
            };
            tab.Controls.Add(txtCheatCode);

            Button btnExec = CreateStyledButton("⚡ Execute Cheat Code", 610, 48, 250, 32, Color.FromArgb(124, 58, 237));
            btnExec.Click += delegate { ExecuteCheatCode(txtCheatCode.Text); };
            tab.Controls.Add(btnExec);

            // Quick Cheat Code library
            GroupBox grpCheatList = new GroupBox();
            grpCheatList.Text = "Supported Cheat Codes (Click to Copy)";
            grpCheatList.ForeColor = Color.FromArgb(129, 140, 248);
            grpCheatList.Location = new Point(20, 95);
            grpCheatList.Size = new Size(370, 355);
            tab.Controls.Add(grpCheatList);

            ListBox listSupported = new ListBox();
            listSupported.Location = new Point(15, 25);
            listSupported.Size = new Size(340, 315);
            listSupported.BackColor = Color.FromArgb(32, 35, 44);
            listSupported.ForeColor = Color.FromArgb(251, 191, 36);
            listSupported.Font = new Font("Consolas", 9.5f);
            listSupported.Items.Add("POKEMON MEWTWO 100 SHINY");
            listSupported.Items.Add("POKEMON CHARIZARD 100 SHINY");
            listSupported.Items.Add("POKEMON RAYQUAZA 100");
            listSupported.Items.Add("POKEMON PIKACHU 50 SHINY");
            listSupported.Items.Add("MASTERBALL 99");
            listSupported.Items.Add("RARECANDY 99");
            listSupported.Items.Add("SHINY ALL");
            listSupported.Items.Add("LEVEL 100");
            listSupported.Items.Add("HEAL");
            listSupported.Items.Add("ALLSTONES 20");
            listSupported.Items.Add("GIVE <item> <qty>");
            listSupported.Items.Add("SET TIME DAY");
            listSupported.Items.Add("SET TIME NIGHT");
            listSupported.Items.Add("MAXITEMS");
            listSupported.DoubleClick += delegate {
                if (listSupported.SelectedItem != null) {
                    txtCheatCode.Text = listSupported.SelectedItem.ToString();
                }
            };
            grpCheatList.Controls.Add(listSupported);

            // Log output
            GroupBox grpLog = new GroupBox();
            grpLog.Text = "Cheat Console Output";
            grpLog.ForeColor = Color.FromArgb(129, 140, 248);
            grpLog.Location = new Point(410, 95);
            grpLog.Size = new Size(460, 355);
            tab.Controls.Add(grpLog);

            listCheatLog = new ListBox();
            listCheatLog.Location = new Point(15, 25);
            listCheatLog.Size = new Size(430, 315);
            listCheatLog.BackColor = Color.FromArgb(18, 20, 24);
            listCheatLog.ForeColor = Color.FromArgb(52, 211, 153);
            listCheatLog.Font = new Font("Consolas", 9f);
            listCheatLog.Items.Add("[System] Mod Menu & Spawner Console Ready.");
            grpLog.Controls.Add(listCheatLog);
        }

        // ----------------------------------------------------
        // TAB 4: WORLD CHEATS
        // ----------------------------------------------------
        private void InitWorldTab()
        {
            TabPage tab = new TabPage("World");
            tab.BackColor = Color.FromArgb(24, 26, 32);
            tab.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            tabControl.TabPages.Add(tab);

            GroupBox grpTime = new GroupBox();
            grpTime.Text = "Time of Day";
            grpTime.ForeColor = Color.FromArgb(129, 140, 248);
            grpTime.Location = new Point(20, 20);
            grpTime.Size = new Size(350, 160);
            tab.Controls.Add(grpTime);

            radDay = new RadioButton();
            radDay.Text = "☀️ Day";
            radDay.Location = new Point(25, 35);
            radDay.Checked = true;
            grpTime.Controls.Add(radDay);

            radNight = new RadioButton();
            radNight.Text = "🌙 Night (Ghost/Dark Spawns)";
            radNight.Location = new Point(25, 75);
            grpTime.Controls.Add(radNight);

            radDusk = new RadioButton();
            radDusk.Text = "🌅 Dusk / Sunset";
            radDusk.Location = new Point(25, 115);
            grpTime.Controls.Add(radDusk);

            Button btnApplyTime = CreateStyledButton("Apply Time of Day", 390, 50, 180, 36, Color.FromArgb(245, 158, 11));
            btnApplyTime.Click += delegate {
                if (saveData != null) {
                    if (radDay.Checked) saveData["timeOfDay"] = "day";
                    else if (radNight.Checked) saveData["timeOfDay"] = "night";
                    else if (radDusk.Checked) saveData["timeOfDay"] = "dusk";
                    ApplyAndSave("Time of day set to " + saveData["timeOfDay"]);
                }
            };
            tab.Controls.Add(btnApplyTime);

            GroupBox grpBackup = new GroupBox();
            grpBackup.Text = "Save Backups & Safety";
            grpBackup.ForeColor = Color.FromArgb(129, 140, 248);
            grpBackup.Location = new Point(20, 200);
            grpBackup.Size = new Size(550, 120);
            tab.Controls.Add(grpBackup);

            Label lblBackupInfo = new Label();
            lblBackupInfo.Text = "Before any cheat is applied, an automatic backup (.bak) is generated in your .sav folder. If you ever want to restore your original save:";
            lblBackupInfo.Location = new Point(15, 25);
            lblBackupInfo.Size = new Size(520, 40);
            grpBackup.Controls.Add(lblBackupInfo);

            Button btnRestore = CreateStyledButton("⏮ Restore from .bak Backup", 15, 70, 220, 34, Color.FromArgb(239, 68, 68));
            btnRestore.Click += delegate { RestoreBackup(); };
            grpBackup.Controls.Add(btnRestore);
        }

        // ----------------------------------------------------
        // DATA LOGIC & SAVE HANDLING
        // ----------------------------------------------------
        private void ScanSaves()
        {
            cmbSaves.Items.Clear();
            if (!Directory.Exists(gameDir)) return;

            string[] dirs = Directory.GetDirectories(gameDir, "*.sav");
            foreach (string d in dirs)
            {
                cmbSaves.Items.Add(Path.GetFileName(d));
            }

            if (cmbSaves.Items.Count > 0)
            {
                cmbSaves.SelectedIndex = 0;
            }
            else
            {
                lblPlayerInfo.Text = "No .sav folder found! Please launch game & click Save once.";
            }
        }

        private void LoadSelectedSave()
        {
            if (cmbSaves.SelectedItem == null) return;
            activeSavFolder = Path.Combine(gameDir, cmbSaves.SelectedItem.ToString());
            activeZipPath = Path.Combine(activeSavFolder, "game.json.zip");

            if (!File.Exists(activeZipPath))
            {
                lblPlayerInfo.Text = "game.json.zip missing in " + cmbSaves.SelectedItem;
                return;
            }

            try
            {
                string json = "";
                using (var zip = ZipFile.OpenRead(activeZipPath))
                {
                    var entry = zip.GetEntry("data.json");
                    if (entry == null) entry = zip.GetEntry("game.json");
                    if (entry != null)
                    {
                        using (var reader = new StreamReader(entry.Open()))
                        {
                            json = reader.ReadToEnd();
                        }
                    }
                }

                saveData = jsonSer.Deserialize<Dictionary<string, object>>(json);
                RefreshUIFromData();
                lblStatus.Text = "Loaded: " + cmbSaves.SelectedItem + " successfully.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load save: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshUIFromData()
        {
            if (saveData == null) return;

            Dictionary<string, object> player = saveData["playerData"] as Dictionary<string, object>;
            if (player != null)
            {
                string pName = player.ContainsKey("name") ? player["name"].ToString() : "Player";
                string map = saveData.ContainsKey("currMapId") ? saveData["currMapId"].ToString() : "0,0";
                lblPlayerInfo.Text = string.Format("Player: {0} | Map: {1} | World: {2}", pName, map, Path.GetFileName(activeSavFolder));

                // Items list
                listItems.Items.Clear();
                if (player.ContainsKey("itemsDict"))
                {
                    Dictionary<string, object> items = player["itemsDict"] as Dictionary<string, object>;
                    if (items != null)
                    {
                        foreach (var kv in items)
                        {
                            ListViewItem lvi = new ListViewItem(kv.Key);
                            lvi.SubItems.Add(kv.Value.ToString());
                            listItems.Items.Add(lvi);
                        }
                    }
                }

                // Party list
                listParty.Items.Clear();
                if (player.ContainsKey("pokemon"))
                {
                    object[] pokeList = player["pokemon"] as object[];
                    if (pokeList != null)
                    {
                        for (int i = 0; i < pokeList.Length; i++)
                        {
                            var pDict = pokeList[i] as Dictionary<string, object>;
                            if (pDict != null)
                            {
                                string pNick = pDict.ContainsKey("nickname") ? pDict["nickname"].ToString() : "Pokemon";
                                string pLvl = pDict.ContainsKey("level") ? pDict["level"].ToString() : "1";
                                bool isShiny = pDict.ContainsKey("isShiny") && Convert.ToBoolean(pDict["isShiny"]);
                                string shinyBadge = isShiny ? "✨ " : "";
                                listParty.Items.Add(string.Format("{0}. {1}{2} (Lvl {3})", i + 1, shinyBadge, pNick, pLvl));
                            }
                        }
                    }
                }
                if (listParty.Items.Count > 0) listParty.SelectedIndex = 0;

                // Time of day
                if (saveData.ContainsKey("timeOfDay"))
                {
                    string tod = saveData["timeOfDay"].ToString().ToLower();
                    if (tod == "night") radNight.Checked = true;
                    else if (tod == "dusk") radDusk.Checked = true;
                    else radDay.Checked = true;
                }
            }
        }

        private void DisplaySelectedPokemon()
        {
            if (listParty.SelectedIndex < 0 || saveData == null) return;
            var player = saveData["playerData"] as Dictionary<string, object>;
            if (player == null) return;
            var pokeList = player["pokemon"] as object[];
            if (pokeList == null || listParty.SelectedIndex >= pokeList.Length) return;

            var p = pokeList[listParty.SelectedIndex] as Dictionary<string, object>;
            if (p == null) return;

            txtPokeNick.Text = p.ContainsKey("nickname") ? p["nickname"].ToString() : "";
            numPokeLevel.Value = p.ContainsKey("level") ? Convert.ToInt32(p["level"]) : 1;
            numPokeHp.Value = p.ContainsKey("hp") ? Convert.ToInt32(p["hp"]) : 20;
            numPokeFriend.Value = p.ContainsKey("friendliness") ? Convert.ToInt32(p["friendliness"]) : 70;
            chkPokeShiny.Checked = p.ContainsKey("isShiny") && Convert.ToBoolean(p["isShiny"]);

            txtMove1.Clear(); txtMove2.Clear(); txtMove3.Clear(); txtMove4.Clear();
            if (p.ContainsKey("attacks"))
            {
                object[] att = p["attacks"] as object[];
                if (att != null)
                {
                    if (att.Length > 0 && att[0] != null) txtMove1.Text = att[0].ToString();
                    if (att.Length > 1 && att[1] != null) txtMove2.Text = att[1].ToString();
                    if (att.Length > 2 && att[2] != null) txtMove3.Text = att[2].ToString();
                    if (att.Length > 3 && att[3] != null) txtMove4.Text = att[3].ToString();
                }
            }
        }

        private void SaveSelectedPokemonDetails()
        {
            if (listParty.SelectedIndex < 0 || saveData == null) return;
            var player = saveData["playerData"] as Dictionary<string, object>;
            if (player == null) return;
            var pokeList = player["pokemon"] as object[];
            if (pokeList == null || listParty.SelectedIndex >= pokeList.Length) return;

            var p = pokeList[listParty.SelectedIndex] as Dictionary<string, object>;
            if (p == null) return;

            p["nickname"] = txtPokeNick.Text.Trim();
            p["level"] = (int)numPokeLevel.Value;
            p["hp"] = (int)numPokeHp.Value;
            p["friendliness"] = (int)numPokeFriend.Value;
            p["isShiny"] = chkPokeShiny.Checked;

            p["attacks"] = new object[] {
                string.IsNullOrEmpty(txtMove1.Text.Trim()) ? null : (object)txtMove1.Text.Trim().ToLower(),
                string.IsNullOrEmpty(txtMove2.Text.Trim()) ? null : (object)txtMove2.Text.Trim().ToLower(),
                string.IsNullOrEmpty(txtMove3.Text.Trim()) ? null : (object)txtMove3.Text.Trim().ToLower(),
                string.IsNullOrEmpty(txtMove4.Text.Trim()) ? null : (object)txtMove4.Text.Trim().ToLower()
            };

            // Also update currPokemon if it is slot 0
            if (listParty.SelectedIndex == 0 && player.ContainsKey("currPokemon"))
            {
                player["currPokemon"] = p;
            }

            ApplyAndSave("Updated " + txtPokeNick.Text);
            RefreshUIFromData();
        }

        // ----------------------------------------------------
        // POKEMON SPAWNER & MANAGEMENT
        // ----------------------------------------------------
        public void AddNewPokemonToParty(string pokeName, int level, bool isShiny, string gender, string nick)
        {
            if (saveData == null) return;
            var player = saveData["playerData"] as Dictionary<string, object>;
            if (player == null) return;

            List<object> list = new List<object>();
            if (player.ContainsKey("pokemon") && player["pokemon"] is object[])
            {
                list.AddRange((object[])player["pokemon"]);
            }

            if (list.Count >= 6)
            {
                MessageBox.Show("Your Party is FULL (6/6 Pokémon)!\n\nPlease release/delete a Pokémon from your party first using the 'Release / Delete Selected' button.", "Party Full", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string cleanName = pokeName.Trim().ToLower();
            string cleanNick = string.IsNullOrEmpty(nick) ? cleanName : nick.Trim();
            int hpVal = Math.Max(40, level * 3 + 25);
            int expVal = level * level * level;

            Dictionary<string, object> newPoke = new Dictionary<string, object>();
            newPoke["name"] = cleanName;
            newPoke["nickname"] = cleanNick;
            newPoke["level"] = level;
            newPoke["hp"] = hpVal;
            newPoke["exp"] = expVal;
            newPoke["gender"] = string.IsNullOrEmpty(gender) ? "male" : gender.ToLower();
            newPoke["friendliness"] = 150;
            newPoke["aggroPlayer"] = false;
            newPoke["test"] = false;
            newPoke["generation"] = "CRYSTAL";
            newPoke["isShiny"] = isShiny;
            newPoke["attacks"] = new object[] { "flamethrower", "thunderbolt", "earthquake", "psychic" };
            newPoke["index"] = list.Count;
            newPoke["previousOwnerName"] = player.ContainsKey("name") ? player["name"].ToString() : "adi";
            newPoke["position"] = 0;
            newPoke["interiorIndex"] = 100;
            newPoke["isInterior"] = false;
            newPoke["harvestTimer"] = 0;

            list.Add(newPoke);
            player["pokemon"] = list.ToArray();

            if (list.Count == 1 || !player.ContainsKey("currPokemon"))
            {
                player["currPokemon"] = newPoke;
            }

            string shinyText = isShiny ? "✨ Shiny " : "";
            ApplyAndSave(string.Format("Added {0}{1} (Lvl {2}) to Party!", shinyText, cleanNick, level));
            RefreshUIFromData();
            listParty.SelectedIndex = list.Count - 1;
        }

        private void ReleaseSelectedPokemon()
        {
            if (listParty.SelectedIndex < 0 || saveData == null) return;
            var player = saveData["playerData"] as Dictionary<string, object>;
            if (player == null) return;

            List<object> list = new List<object>();
            if (player.ContainsKey("pokemon") && player["pokemon"] is object[])
            {
                list.AddRange((object[])player["pokemon"]);
            }

            if (list.Count <= 1)
            {
                MessageBox.Show("You cannot release your last Pokémon! You must keep at least 1 Pokémon in your party.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int selIdx = listParty.SelectedIndex;
            var pDict = list[selIdx] as Dictionary<string, object>;
            string pName = pDict != null && pDict.ContainsKey("nickname") ? pDict["nickname"].ToString() : "Pokemon";

            DialogResult res = MessageBox.Show(string.Format("Are you sure you want to release {0} from your party?", pName), "Confirm Release", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res != DialogResult.Yes) return;

            list.RemoveAt(selIdx);
            for (int i = 0; i < list.Count; i++)
            {
                var p = list[i] as Dictionary<string, object>;
                if (p != null) p["index"] = i;
            }
            player["pokemon"] = list.ToArray();

            if (player.ContainsKey("currPokemon"))
            {
                player["currPokemon"] = list[0];
            }

            ApplyAndSave(string.Format("Released {0} from party.", pName));
            RefreshUIFromData();
        }

        private void MakeEntirePartyShiny()
        {
            if (saveData == null) return;
            var player = saveData["playerData"] as Dictionary<string, object>;
            if (player == null) return;
            var pokeList = player["pokemon"] as object[];
            if (pokeList == null) return;

            foreach (var pObj in pokeList)
            {
                var p = pObj as Dictionary<string, object>;
                if (p != null) p["isShiny"] = true;
            }
            if (player.ContainsKey("currPokemon") && player["currPokemon"] is Dictionary<string, object>)
            {
                ((Dictionary<string, object>)player["currPokemon"])["isShiny"] = true;
            }

            ApplyAndSave("All Party Pokémon are now SHINY! ✨");
            RefreshUIFromData();
        }

        private void Level100EntireParty()
        {
            if (saveData == null) return;
            var player = saveData["playerData"] as Dictionary<string, object>;
            if (player == null) return;
            var pokeList = player["pokemon"] as object[];
            if (pokeList == null) return;

            foreach (var pObj in pokeList)
            {
                var p = pObj as Dictionary<string, object>;
                if (p != null)
                {
                    p["level"] = 100;
                    p["hp"] = 350;
                    p["exp"] = 1000000;
                }
            }
            ApplyAndSave("All Party Pokémon set to Level 100! ⚡");
            RefreshUIFromData();
        }

        private void AddItemDirectly(string itemName, int count)
        {
            if (saveData == null) return;
            var player = saveData["playerData"] as Dictionary<string, object>;
            if (player == null) return;

            Dictionary<string, object> items;
            if (!player.ContainsKey("itemsDict") || !(player["itemsDict"] is Dictionary<string, object>))
            {
                items = new Dictionary<string, object>();
                player["itemsDict"] = items;
            }
            else
            {
                items = (Dictionary<string, object>)player["itemsDict"];
            }

            if (items.ContainsKey(itemName))
            {
                items[itemName] = Convert.ToInt32(items[itemName]) + count;
            }
            else
            {
                items[itemName] = count;
            }

            ApplyAndSave(string.Format("+{0} {1} added to bag!", count, itemName));
            RefreshUIFromData();
        }

        private void AddAllEvoStones(int count)
        {
            string[] stones = new string[] {
                "fire stone", "water stone", "thunderstone", "leaf stone", "moon stone", 
                "sun stone", "dawn stone", "dusk stone", "shiny stone", "ice stone", "hard stone"
            };
            foreach (string s in stones) AddItemDirectly(s, count);
        }

        private void AddBerriesAndRopes(int count)
        {
            string[] items = new string[] {
                "escape rope", "lum berry", "pecha berry", "cheri berry", "aspear berry", "chesto berry", "rawst berry", "persim berry"
            };
            foreach (string itm in items) AddItemDirectly(itm, count);
        }

        private void AddAllPokeballs(int count)
        {
            string[] balls = new string[] {
                "master ball", "ultra ball", "dusk ball", "heavy ball", "moon ball", "fast ball", "love ball", "level ball", "net ball"
            };
            foreach (string b in balls) AddItemDirectly(b, count);
        }

        private void MaxOutAllItems()
        {
            if (saveData == null) return;
            var player = saveData["playerData"] as Dictionary<string, object>;
            if (player == null || !player.ContainsKey("itemsDict")) return;
            var items = player["itemsDict"] as Dictionary<string, object>;
            if (items == null) return;

            List<string> keys = new List<string>(items.Keys);
            foreach (string k in keys) items[k] = 999;

            ApplyAndSave("Maxed out all items in bag to 999!");
            RefreshUIFromData();
        }

        private void RemoveSelectedItem()
        {
            if (listItems.SelectedItems.Count == 0 || saveData == null) return;
            string itm = listItems.SelectedItems[0].Text;
            var player = saveData["playerData"] as Dictionary<string, object>;
            if (player == null) return;
            var items = player["itemsDict"] as Dictionary<string, object>;
            if (items != null && items.ContainsKey(itm))
            {
                items.Remove(itm);
                ApplyAndSave("Removed " + itm);
                RefreshUIFromData();
            }
        }

        private void AdjustSelectedItem(int amount)
        {
            if (listItems.SelectedItems.Count == 0 || saveData == null) return;
            string itm = listItems.SelectedItems[0].Text;
            AddItemDirectly(itm, amount);
        }

        private void SetSelectedItemQty(int target)
        {
            if (listItems.SelectedItems.Count == 0 || saveData == null) return;
            string itm = listItems.SelectedItems[0].Text;
            var player = saveData["playerData"] as Dictionary<string, object>;
            if (player == null) return;
            var items = player["itemsDict"] as Dictionary<string, object>;
            if (items != null)
            {
                items[itm] = target;
                ApplyAndSave(string.Format("Set {0} to {1}", itm, target));
                RefreshUIFromData();
            }
        }

        // ----------------------------------------------------
        // CHEAT CODE PARSER
        // ----------------------------------------------------
        private void ExecuteCheatCode(string rawCode)
        {
            if (string.IsNullOrEmpty(rawCode)) return;
            string code = rawCode.Trim().ToUpper();
            string[] parts = code.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;

            string cmd = parts[0];
            try
            {
                if (cmd == "POKEMON" || cmd == "SPAWN")
                {
                    if (parts.Length >= 2)
                    {
                        string pName = parts[1].ToLower();
                        int lvl = 50;
                        bool shiny = false;
                        if (parts.Length >= 3) int.TryParse(parts[2], out lvl);
                        if (parts.Length >= 4 && parts[3] == "SHINY") shiny = true;
                        if (code.Contains("SHINY")) shiny = true;

                        AddNewPokemonToParty(pName, lvl, shiny, "male", pName);
                        LogCheat(string.Format("[ACTIVATED] Spawned {0} (Lvl {1}, Shiny: {2})", pName, lvl, shiny));
                    }
                }
                else if (cmd == "MASTERBALL" || cmd == "MASTER_BALL")
                {
                    int qty = parts.Length > 1 ? int.Parse(parts[1]) : 99;
                    AddItemDirectly("master ball", qty);
                    LogCheat(string.Format("[ACTIVATED] +{0} Master Balls added!", qty));
                }
                else if (cmd == "RARECANDY" || cmd == "RARE_CANDY")
                {
                    int qty = parts.Length > 1 ? int.Parse(parts[1]) : 99;
                    AddItemDirectly("rare candy", qty);
                    LogCheat(string.Format("[ACTIVATED] +{0} Rare Candies added!", qty));
                }
                else if (cmd == "SHINY")
                {
                    MakeEntirePartyShiny();
                    LogCheat("[ACTIVATED] All Party Pokémon turned SHINY!");
                }
                else if (cmd == "LEVEL" || cmd == "LEVEL100" || cmd == "MAXLEVEL")
                {
                    int lvl = parts.Length > 1 ? int.Parse(parts[1]) : 100;
                    if (saveData != null)
                    {
                        var player = saveData["playerData"] as Dictionary<string, object>;
                        var pokeList = player["pokemon"] as object[];
                        foreach (var p in pokeList) ((Dictionary<string, object>)p)["level"] = lvl;
                        ApplyAndSave("Set party level to " + lvl);
                        RefreshUIFromData();
                    }
                    LogCheat("[ACTIVATED] Party Level set to " + lvl);
                }
                else if (cmd == "HEAL")
                {
                    if (saveData != null)
                    {
                        var player = saveData["playerData"] as Dictionary<string, object>;
                        var pokeList = player["pokemon"] as object[];
                        foreach (var p in pokeList) ((Dictionary<string, object>)p)["hp"] = 350;
                        ApplyAndSave("Fully healed all party Pokemon!");
                        RefreshUIFromData();
                    }
                    LogCheat("[ACTIVATED] Party fully healed!");
                }
                else if (cmd == "ALLSTONES")
                {
                    int qty = parts.Length > 1 ? int.Parse(parts[1]) : 20;
                    AddAllEvoStones(qty);
                    LogCheat(string.Format("[ACTIVATED] +{0} All Evolution Stones added!", qty));
                }
                else if (cmd == "MAXITEMS")
                {
                    MaxOutAllItems();
                    LogCheat("[ACTIVATED] Maxed out all bag items to 999!");
                }
                else if (cmd == "GIVE")
                {
                    if (parts.Length >= 2)
                    {
                        int qty = parts.Length >= 3 ? int.Parse(parts[parts.Length - 1]) : 99;
                        int nameTokens = parts.Length >= 3 ? parts.Length - 2 : parts.Length - 1;
                        string itm = "";
                        for (int i = 1; i <= nameTokens; i++) itm += parts[i].ToLower() + " ";
                        itm = itm.Trim();
                        AddItemDirectly(itm, qty);
                        LogCheat(string.Format("[ACTIVATED] Given {0} x{1}", itm, qty));
                    }
                }
                else if (cmd == "SET" && parts.Length >= 3 && parts[1] == "TIME")
                {
                    string tod = parts[2].ToLower();
                    if (saveData != null)
                    {
                        saveData["timeOfDay"] = tod;
                        ApplyAndSave("Time of day set to " + tod);
                        RefreshUIFromData();
                    }
                    LogCheat("[ACTIVATED] Time set to " + tod);
                }
                else
                {
                    LogCheat("[ERROR] Unknown cheat code: " + rawCode);
                }
            }
            catch (Exception ex)
            {
                LogCheat("[ERROR] " + ex.Message);
            }
            txtCheatCode.Clear();
        }

        private void LogCheat(string msg)
        {
            listCheatLog.Items.Add(string.Format("[{0:HH:mm:ss}] {1}", DateTime.Now, msg));
            listCheatLog.SelectedIndex = listCheatLog.Items.Count - 1;
        }

        // ----------------------------------------------------
        // SAVE & BACKUP OPERATIONS
        // ----------------------------------------------------
        private void ApplyAndSave(string successMessage)
        {
            if (saveData == null || string.IsNullOrEmpty(activeZipPath)) return;

            try
            {
                string bakPath = activeZipPath + ".bak";
                if (File.Exists(activeZipPath))
                {
                    File.Copy(activeZipPath, bakPath, true);
                }

                string newJson = jsonSer.Serialize(saveData);

                using (var zip = ZipFile.Open(activeZipPath, ZipArchiveMode.Update))
                {
                    var oldEntry = zip.GetEntry("data.json");
                    if (oldEntry == null) oldEntry = zip.GetEntry("game.json");
                    if (oldEntry != null) oldEntry.Delete();

                    var newEntry = zip.CreateEntry("data.json", CompressionLevel.Optimal);
                    using (var writer = new StreamWriter(newEntry.Open()))
                    {
                        writer.Write(newJson);
                    }
                }

                lblStatus.Text = successMessage + " (Saved at " + DateTime.Now.ToString("HH:mm:ss") + ")";
                lblStatus.ForeColor = Color.FromArgb(52, 211, 153);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Save Error: " + ex.Message;
                lblStatus.ForeColor = Color.FromArgb(239, 68, 68);
                MessageBox.Show("Failed to save changes: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RestoreBackup()
        {
            if (string.IsNullOrEmpty(activeZipPath)) return;
            string bakPath = activeZipPath + ".bak";
            if (!File.Exists(bakPath))
            {
                MessageBox.Show("No backup file found (.bak)!", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                File.Copy(bakPath, activeZipPath, true);
                LoadSelectedSave();
                MessageBox.Show("Original save restored successfully from backup!", "Restored", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Restore failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Button CreateStyledButton(string text, int x, int y, int w, int h, Color bg)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(x, y);
            btn.Size = new Size(w, h);
            btn.BackColor = bg;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;
            btn.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            return btn;
        }
    }
}
