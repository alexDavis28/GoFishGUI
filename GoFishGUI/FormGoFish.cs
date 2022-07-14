using System;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;

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
        int books_to_win = 3;

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
            CheckIfPackEmpty();
            game.DealCardToHand(0);
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

        private bool RequestCard(int requested_card_rank)
        {
            CheckIfPackEmpty();
            bool is_catch = true;
            string rank_as_string = game.PluralRankAsString(requested_card_rank);
            bool has_card = game.HasCardResponse(game.next_turn, requested_card_rank);

            // Current player asks for card
            dialogues[game.current_turn].Text = "Do you have any " + rank_as_string;
            dialogues[game.current_turn].Refresh();
            
            if (has_card)
            {
                dialogues[game.next_turn].Text = "Yes";
                int num = game.TransferCards(game.current_turn, game.next_turn, requested_card_rank);
                dialogues[game.current_turn].Text += $"\n<Recieved {num} {rank_as_string}>";
            }
            else
            {
                dialogues[game.next_turn].Text = "I don't, Go Fish";
                Card c = game.DealCardToHand(game.current_turn);
                dialogues[game.current_turn].Text += $"\n<Recieved a {c.GetName()}>";

                if (c.GetRank() == requested_card_rank)
                {
                    dialogues[game.current_turn].Text += "\nCatch!";
                }
                else
                {
                    IncrementTurn();
                    is_catch = false;
                }
            }
            return is_catch;
        }

        private void NextTurn()
        {
            HideDialogues();
            dialogues[game.current_turn].Visible = true;
            dialogues[game.next_turn].Visible = true;
            
            CurrentTurnLabel.Text = "Current turn: " + Convert.ToString(game.current_turn + 1);

            if (game.current_turn != 0)
            {
                // Ai turn
                int requested_card_rank = game.ComputerRequestCard(game.current_turn);
                RequestCard(requested_card_rank);
            }
            else
            {
                // Plauer turn
                NextTurnButton.Enabled = false;
                DrawButton.Enabled = true;
                RequestCardButton.Enabled = true;
            }
            FormBooks();
            DrawPlayerCardImages();
        }

        private void PlayerRequestCard()
        {
            int requested_card_rank;
            try
            {
                requested_card_rank = Convert.ToInt32(RequestCardInput.Text);
            }
            catch (FormatException e)
            {
                // Handling for non-integer rank requests (ie: king, jack)
                requested_card_rank = InterpretRequestedRank(RequestCardInput.Text);
            }

            if (!game.Hands[0].HasCard(requested_card_rank))
            {
                MessageBox.Show("You don't have a card of that rank");
            }
            else
            {
                dialogues[game.current_turn].Visible = true;
                dialogues[game.next_turn].Visible = true;

                bool is_catch = RequestCard(requested_card_rank);

                if (!is_catch)
                {
                    EndPlayerTurn();
                }
                FormBooks();
                DrawPlayerCardImages();
            }
            RequestCardInput.Clear();
        }

        // Gameplay admin

        private void FormBooks()
        {
            //Check for sets of 4 cards of same rank
            game.Hands[game.current_turn].FormBooks();
            DrawPlayerCardImages();
            scores[game.current_turn].Text = "Score: " + Convert.ToString(game.Hands[game.current_turn].BookCount);
        }

        private void IncrementTurn()
        {
            CheckForWinner();
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

        private void EndGame(int winner, int score)
        {
            string message = $"{winner} has won the game with {score} books!";
            MessageBox.Show(message);
            NewGame();
        }

        private void EndGame(int winner, int score, bool draw)
        {
            string message;
            if (draw)
            {
                message = $"The pack is empty, the result is a draw";
            }
            else
            {
                message = $"The pack is empty, Player {winner} has won the game with {score} book(s)!";
            }
            MessageBox.Show(message);
            NewGame();
            
        }

        private void CheckForWinner()
        {
            for (int i = 0; i < game.Hands.Length; i++)
            {
                if (game.Hands[i].BookCount >= books_to_win)
                {
                    EndGame(i + 1, game.Hands[i].BookCount);
                }
            }
        }

        private void CheckIfPackEmpty()
        {
            if (game.PackSize <= 0)
            {
                int[] scores = new int[4];
                int count = 0;
                bool draw = false;

                for (int i = 0; i < game.Hands.Length; i++)
                {
                    scores[i] = game.Hands[i].BookCount;
                }
                int max_score = scores.Max();
                int winning_player = scores.ToList().IndexOf(max_score);

                for (int i = 0; i < scores.Length; i++)
                {
                    if (scores[i] == max_score)
                    {
                        count++;
                    }
                }
                if (count > 1 || max_score == 0)
                {
                    draw = true;
                }

                EndGame(winning_player+1, max_score, draw);
            }
        }

        private int InterpretRequestedRank(string rank)
        {
            rank.ToLower();

            //Account for plurals
            if (rank.EndsWith("s"))
            {
                rank = rank.Substring(0, rank.Length - 1);
            }
            int output;

            Dictionary<string, int> dict = new Dictionary<string, int>() {
                {"ace", 1},
                {"two", 2},
                {"three", 3},
                {"four", 4},
                {"five", 5},
                {"six", 6},
                {"seven", 7},
                {"eight", 8},
                {"nine", 9},
                {"ten", 10},
                {"jack", 11},
                {"queen", 12},
                {"king", 13}
            };

            bool valid_key = dict.TryGetValue(rank, out output);

            if (!valid_key)
            {
                output = -1;
            }

            return output;
        }
    }
}
