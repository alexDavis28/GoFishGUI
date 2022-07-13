using System;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;

// TODO: deck empty, hand empty, win condition, notification for book forming, display formed books, can ask for cards you dont have, better intperetation of requested card strings, clear request box on request

namespace GoFishGUI
{
    public partial class FormGoFish : Form
    {

        ResourceManager rm = Resources.ResourceManager;
        Graphics g;
        GoFish game;
        Label[] dialogues = new Label[4];
        Label[] scores = new Label[4];
        int cards_in_a_row = 9;

        public FormGoFish()
        {
            InitializeComponent();
        }

        private void NewGameButton_Click(object sender, EventArgs e)
        {
            NewGame();
        }

        // Setup
        private void FormGoFish_Load(object sender, EventArgs e)
        {
            g = CreateGraphics();
            P1CardImages.ImageSize = new Size(72, 96);

            // Create lists of the dialogue boxes and scores for each player to make them more dynamically accessible
            dialogues = new Label[] { P1Dialogue, P2Dialogue, P3Dialogue, P4Dialogue };
            scores = new Label[] { P1Score, P2Score, P3Score, P4Score };

            NewGame();
        }

        private void NewGame()
        {
            game = new GoFish();
            game.ShuffleCards();
            game.DealCards();

            game.current_turn = 0;
            game.next_turn = 1;

            for (int i = 0; i < 4; i++)
            {
                HideDialogues();
                scores[i].Text = "Score: 0";
            }
            DrawPlayerCardImages();
            NextTurn();
        }

        // Graphics

        private void HideDialogues()
        {
            for (int i = 0; i < 4; i++)
            {
                dialogues[i].Visible = false;
                dialogues[i].Text = "";
            }
        }

        public void DrawPlayerCardImages()
        {
            game.PlayerSortHand();
            GoFishHand player_hand = game.Hands[0];
            P1CardImages.Images.Clear();

            for (int i = 0; i < player_hand.Size; i++)
            {
                CreateImage(player_hand[i]);
            }
        }

        private void CreateImage(Card card)
        {
            string imagename = "_" + card.GetRank().ToString() + card.GetSuitAsString().ToLower()[0];
            Image cardimage = (Image)rm.GetObject(imagename);
            P1CardImages.Images.Add(cardimage);

            Invalidate();
        }

        private void FormGoFish_Paint(object sender, PaintEventArgs e)
        {
            for (int i = 0; i < P1CardImages.Images.Count; i++)
            {
                g.DrawImage(P1CardImages.Images[i], 100 + 100 * (i % cards_in_a_row), 500 + (100 * (i / cards_in_a_row)), 72, 96);
            }
        }

        // Events

        private void DrawButton_Click(object sender, EventArgs e)
        {
            game.DealCard(0);
            EndPlayerTurn();
            IncrementTurn();
            HideDialogues();
        }

        private void NextTurnButton_Click(object sender, EventArgs e)
        {
            NextTurn();
        }

        private void RequestCardButton_Click(object sender, EventArgs e)
        {
            PlayerRequestCard();
        }

        // Gameplay

        private void NextTurn()
        {
            HideDialogues();

            dialogues[game.current_turn].Visible = true;
            dialogues[game.next_turn].Visible = true;

            CurrentTurnLabel.Text = "Current turn: " + Convert.ToString(game.current_turn + 1);

            if (game.current_turn != 0) // Computer turns
            {
                int requested_card_rank = game.ComputerRequestCard(game.current_turn);
                string rank_as_string = game.RankAsString(requested_card_rank);
                dialogues[game.current_turn].Text = "Do you have any " + rank_as_string;
                dialogues[game.current_turn].Refresh();
                bool has_card = game.HasCardResponse(game.next_turn, requested_card_rank);
                if (has_card)
                {
                    dialogues[game.next_turn].Text = "Yes";
                    int num = game.TransferCards(game.current_turn, game.next_turn, requested_card_rank);
                    dialogues[game.current_turn].Text += $"\n<Recieved {num} {rank_as_string}>";
                }
                else
                {
                    dialogues[game.next_turn].Text = "Go Fish";
                    Card c = game.DealCard(game.current_turn);
                    dialogues[game.current_turn].Text += $"\n<Recieved a {c.GetName()}>";

                    if (c.GetRank() == requested_card_rank)
                    {
                        dialogues[game.current_turn].Text += "\nCatch!";
                    }
                    else
                    {
                        IncrementTurn();
                    }
                }
            }
            else
            {
                NextTurnButton.Enabled = false;
                DrawButton.Enabled = true;
                RequestCardButton.Enabled = true;
            }
            FormBooks();
            DrawPlayerCardImages();
        }

        private void PlayerRequestCard()
        {
            int requested_card_rank = Convert.ToInt32(RequestCardInput.Text);
            HideDialogues();

            dialogues[game.current_turn].Visible = true;
            dialogues[game.next_turn].Visible = true;


            string rank_as_string = game.RankAsString(requested_card_rank);
            dialogues[game.current_turn].Text = "Do you have any " + rank_as_string;
            dialogues[game.current_turn].Refresh();
            bool has_card = game.HasCardResponse(game.next_turn, requested_card_rank);
            if (has_card)
            {
                dialogues[game.next_turn].Text = "Yes";
                int num = game.TransferCards(game.current_turn, game.next_turn, requested_card_rank);
                dialogues[game.current_turn].Text += $"\n<Recieved {num} {rank_as_string}>";
            }
            else
            {
                dialogues[game.next_turn].Text = "Go Fish";
                Card c = game.DealCard(game.current_turn);
                dialogues[game.current_turn].Text += $"\n<Recieved a {c.GetName()}>";

                if (c.GetRank() == requested_card_rank)
                {
                    dialogues[game.current_turn].Text += "\nCatch!";
                }
                else
                {
                    IncrementTurn();
                    EndPlayerTurn();
                }
            }

            FormBooks();
            DrawPlayerCardImages();

        }

        // Gameplay admin

        private void FormBooks()
        {
            game.Hands[game.current_turn].FormBooks();
            DrawPlayerCardImages();
            scores[game.current_turn].Text = "Score: " + Convert.ToString(game.Hands[game.current_turn].BookCount);
        }

        private void IncrementTurn()
        {
            game.current_turn++;
            game.current_turn %= 4;
            game.next_turn = game.current_turn + 1;
            game.next_turn %= 4;
        }

        private void EndPlayerTurn()
        {
            DrawPlayerCardImages();
            NextTurnButton.Enabled = true;
            DrawButton.Enabled = false;
            RequestCardButton.Enabled = false;
        }
    }
}
