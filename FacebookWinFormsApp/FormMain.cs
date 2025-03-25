using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FacebookWrapper.ObjectModel;
using FacebookWrapper;
using System.Windows.Forms.VisualStyles;
using System.Threading.Tasks;
using System.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using BasicFacebookFeatures.singleton;


namespace BasicFacebookFeatures
{
    public partial class FormMain : Form
    {
        private User m_LoggedInUser;
        private FacebookFeaturesManager m_FBFeaturesManager;

        public FormMain()
        {
            InitializeComponent();
            FacebookService.s_CollectionLimit = 25;
            m_LoggedInUser = null;
            tabMatchMaking.TabPages.Insert(1, pageTabMatchMaker);
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            login();
        }

        private void login()
        {
            try
            {
                UserManager.Instance.UserLogin();
                m_LoggedInUser = UserManager.Instance.LoggedInUser;
                if (m_LoggedInUser != null)
                {
                    buttonLogin.Text = $"Logged in as {m_LoggedInUser.Name}";
                    buttonLogin.BackColor = Color.LightGreen;
                    pictureBoxProfile.ImageLocation = m_LoggedInUser.PictureNormalURL;
                    buttonLogin.Enabled = false;
                    buttonLogout.Enabled = true;
                    tabMatchMaking.Enabled = true;
                    m_FBFeaturesManager = new FacebookFeaturesManager(listBoxFriends, listBoxLikedPages, pictureBoxPhotos, listBoxPost);
                    fetchUserDetailsAndDisplay();
                }
                else
                {
                    MessageBox.Show("Login Failed");
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonLogout_Click(object sender, EventArgs e)
        {
            UserManager.Instance.UserLogout();
            m_LoggedInUser = null;
            buttonLogin.Enabled = true;
            buttonLogin.Text = "Login";
            buttonLogin.BackColor = Color.White;
            buttonLogout.Enabled = false;
            m_FBFeaturesManager.ResetUser();
            pictureBoxProfile.ImageLocation = null;
            textBoxPost.Text = string.Empty;
            labelFullName.Text = "Full Name:";
            labelGender.Text = "Gender:";
            labalBirthday.Text = "Birthday:";
            labelEmail.Text = "Email:";
            listBoxPost.Items.Clear();
            listBoxFriends.Items.Clear();
            listBoxLikedPages.Items.Clear();
            pictureBoxPhotos.Image = null;
            pictureBoxLikedPages.Image = null;
            pictureBoxFriends.Image = null;
            tabMatchMaking.Enabled = false;
            clearAllMatchData();
            pictureBoxGreatest.Image = null;
            textBoxGreatestPost.Text = string.Empty;
            m_FBFeaturesManager.UserPhoto = null;
        }

        private void rightPhotoButton_Click(object sender, EventArgs e)
        {
            m_FBFeaturesManager.ChangePicture(1);
        }

        private void leftPhotoButton_Click(object sender, EventArgs e)
        {
            m_FBFeaturesManager.ChangePicture(-1);
        }
        
        private void friendsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxFriends.SelectedItems.Count == 1)
            {
                User selectedFriend = listBoxFriends.SelectedItem as User;

                pictureBoxFriends.LoadAsync(selectedFriend.PictureNormalURL);
            }
        }

        private void likedPagesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxLikedPages.SelectedItems.Count == 1)
            {
                Page selectedPage = listBoxLikedPages.SelectedItem as Page;

                pictureBoxLikedPages.LoadAsync(selectedPage.PictureNormalURL);
            }
        }

        private void postButton_Click(object sender, EventArgs e)
        {
            try
            {
                Status postedStatus = m_LoggedInUser.PostStatus(textBoxPost.Text);

                MessageBox.Show("Status Posted! ID: " + postedStatus.Id);
            }
            catch (Exception ex)
            {
                listBoxPost.Items.Insert(0, textBoxPost.Text); //Only because FB doesn't permit actually posting, to show as if the new post was added.
                //MessageBox.Show(ex.ToString());
            }
        }

        private void fetchUserDetailsAndDisplay()
        {
            new Thread(fetchUserDetails).Start();
            new Thread(m_FBFeaturesManager.DisplayFriends).Start();
            new Thread(m_FBFeaturesManager.DisplayLikedPages).Start();
            new Thread(m_FBFeaturesManager.DisplayAllPosts).Start();
            new Thread(m_FBFeaturesManager.LoadUserPhotos).Start();
        }

        private void fetchUserDetails()
        {
            string userName, userGender, userBirthday, userEmail;

            userName = m_LoggedInUser.Name;
            labelFullName.Invoke(new Action(() => labelFullName.Text = $"Full Name: {userName ?? string.Empty}"));
            userGender = m_LoggedInUser.Gender?.ToString();
            labelGender.Invoke(new Action(() => labelGender.Text = $"Gender: {userGender ?? string.Empty}"));
            userBirthday = m_LoggedInUser.Birthday;
            labalBirthday.Invoke(new Action(() => labalBirthday.Text = $"Birthday: {userBirthday ?? string.Empty}"));
            userEmail = m_LoggedInUser.Email;
            labelEmail.Invoke(new Action(() => labelEmail.Text = $"Email: {userEmail ?? string.Empty}"));
        }

        private void listBoxMatches_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxMatches.SelectedItems.Count == 1)
            {
                User matchPicked = listBoxMatches.SelectedItem as User;
                if (matchPicked != null)
                {
                    findMatchUIDisplayData(matchPicked);
                }
            }
        }

        private void findMatchUIDisplayData(User i_UserMatched)
        {
            pictureBoxMatch.ImageLocation = i_UserMatched.PictureNormalURL;
            labelMatchFullName.Text = $"Full Name: {i_UserMatched.Name ?? string.Empty}";
            labelMatchBirthday.Text = $"Birthday: {i_UserMatched.Birthday ?? string.Empty}";
            labelMatchLocation.Text = $"Location: {i_UserMatched.Location?.Name ?? string.Empty}";
        }

        private void matchButton_Click(object sender, EventArgs e)
        {
            m_FBFeaturesManager.CreateMatcher();
            m_FBFeaturesManager.FindMatchByData(radioButtonFemale, radioButtonMale, numericUpDownMin, numericUpDownMax, listBoxMatches);
        }

        private void clearAllMatchData()
        {
            pictureBoxMatch.Image = null;
            radioButtonFemale.Checked = true;
            listBoxMatches.Items.Clear();
            labelMatchFullName.Text = "Full Name:";
            labelMatchBirthday.Text = "Birthday:";
            labelMatchLocation.Text = "Location:";

        }

        private void greatestOfTheYearButton_Click(object sender, EventArgs e)
        {
            pictureBoxGreatest.Image = null;
            textBoxGreatestPost.Text = "";
            m_FBFeaturesManager.CreateGreatestOfTheYear();
            m_FBFeaturesManager.FindGreatestOfTheYearByData(numericUpDowmGreatetYear, textBoxGreatestPost, pictureBoxGreatest);
        }
    }
}
