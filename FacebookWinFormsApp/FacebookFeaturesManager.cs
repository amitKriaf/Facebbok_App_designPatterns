using FacebookWrapper.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using static FacebookWrapper.ObjectModel.User;
using BasicFacebookFeatures.singleton;
using BasicFacebookFeatures.Factory;


namespace BasicFacebookFeatures
{
    public class FacebookFeaturesManager
    {
        private ListBox m_AllPostsBox;
        private PictureBox m_PictureBox;
        private List<Photo> m_UserPhotos;
        private int m_CurrentPhotoIndex;
        private ListBox m_FriendsListBox;
        private ListBox m_LikedPagesListBox;
        private MatchFeature m_MatchFeature;
        private GreatestOfTheYearFeature m_GreatestOfTheYearFeature;
        private User m_UserLoggedIn;
        
        public FacebookFeaturesManager(ListBox i_FriendsListBox, ListBox i_LikedPagesListBox, PictureBox i_PictureBox, ListBox i_AllPostsBox)
        {
            m_FriendsListBox = i_FriendsListBox;
            m_LikedPagesListBox = i_LikedPagesListBox;
            m_PictureBox = i_PictureBox;
            m_AllPostsBox = i_AllPostsBox;
            m_UserPhotos = new List<Photo>();
            m_CurrentPhotoIndex = 0;
            m_UserLoggedIn = UserManager.Instance.LoggedInUser;
        }

        public List<Photo> UserPhoto
        {
            set
            {
                m_UserPhotos = value;
            }
        }
        
        public void DisplayLikedPages()
        {
            if (m_UserLoggedIn != null)
            {
                m_LikedPagesListBox.Invoke(new Action(() =>
                {
                    m_LikedPagesListBox.Items.Clear();
                    m_LikedPagesListBox.DisplayMember = "Name";
                    foreach (Page page in m_UserLoggedIn.LikedPages)
                    {
                        m_LikedPagesListBox.Items.Add(page);
                    }

                    if (m_LikedPagesListBox.Items.Count == 0)
                    {
                        MessageBox.Show("No pages to retrieve :(");
                    }
                }));
            }
        }

        public void DisplayFriends()
        {

            if (m_UserLoggedIn != null)
            {
                m_FriendsListBox.Invoke(new Action(() =>
                {
                    m_FriendsListBox.Items.Clear();
                    foreach (User friend in m_UserLoggedIn.Friends)
                    {
                        m_FriendsListBox.Items.Add(friend);
                    }

                    if (m_FriendsListBox.Items.Count == 0)
                    {
                        MessageBox.Show("No friends to retrieve :(");
                    }
                }));
            }
        }
        
        public void LoadUserPhotos()
        {
            m_UserPhotos.Clear();
            m_CurrentPhotoIndex = 0;
            if (m_UserLoggedIn != null)
            {
                try
                {
                    foreach (Album album in m_UserLoggedIn.Albums)
                    {
                        m_UserPhotos.AddRange(album.Photos);
                    }
                }

                catch (Exception ex)
                {
                    MessageBox.Show("Error loading photos.");
                }

                if (m_UserPhotos.Count > 0)
                {
                    DisplayPhoto(m_CurrentPhotoIndex);
                }
            }
        }

        public void DisplayPhoto(int i_Index)
        {
            if (m_UserPhotos.Count > 0 && i_Index < m_UserPhotos.Count)
            {
                m_PictureBox.Invoke(new Action(() => m_PictureBox.ImageLocation = m_UserPhotos[i_Index].PictureNormalURL));
            }
        }

        public void ChangePicture(int i_AddToIndex)
        {
            if (m_UserPhotos.Count > 0)
            {
                m_CurrentPhotoIndex = (m_CurrentPhotoIndex + m_UserPhotos.Count + i_AddToIndex) % m_UserPhotos.Count;
                DisplayPhoto(m_CurrentPhotoIndex);
            }
        }

        public void DisplayAllPosts()
        {
            if (m_UserLoggedIn != null)
            {
                m_AllPostsBox.Invoke(new Action(() =>
                {
                    m_AllPostsBox.Items.Clear();
                    foreach (Post post in m_UserLoggedIn.Posts)
                    {
                        m_AllPostsBox.Items.Add(post);
                    }

                    if (m_AllPostsBox.Items.Count == 0)
                    {
                        MessageBox.Show("No post to retrieve :(");
                    }
                }));
            }
        }

        public void CreateGreatestOfTheYear()
        {
            m_GreatestOfTheYearFeature = (GreatestOfTheYearFeature)FeaturesFactory.CreateFeature(FeaturesFactory.eFeatureType.GreatestOfTheYear);
        }

        public void FindGreatestOfTheYearByData(NumericUpDown i_TheGreatestyearNumeric, TextBox i_GreatestPostTextBox, PictureBox i_GreatestPictureBox)
        {
            try
            {
                if (m_UserLoggedIn == null)
                {
                    throw new ArgumentException("no user is login");
                }
                m_GreatestOfTheYearFeature.Year = (int)i_TheGreatestyearNumeric.Value;
                m_GreatestOfTheYearFeature.Execute(i_GreatestPictureBox, m_UserPhotos, i_GreatestPostTextBox, getPostsList());
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private List<Post> getPostsList()
        {
            List<Post> posts = new List<Post>();

            foreach (Post post in m_UserLoggedIn.Posts)
            {
                posts.Add(post);
            }
            
            return posts;
        }

        public void CreateMatcher()
        {
            m_MatchFeature = (MatchFeature)FeaturesFactory.CreateFeature(FeaturesFactory.eFeatureType.MatchMaker);
        }

        public void FindMatchByData(RadioButton i_FemaleRadioButton, RadioButton i_MaleRadioButton, NumericUpDown i_MinPreference, NumericUpDown i_MaxPreference, ListBox i_MatchesListBox)
        {
            try
            {
                i_MatchesListBox.Items.Clear();
                if (m_UserLoggedIn == null)
                {
                    throw new ArgumentException("No user is login");
                }

                m_MatchFeature.SetGenderFromForm(i_FemaleRadioButton, i_MaleRadioButton);
                m_MatchFeature.MaxAgePreference = (int)i_MaxPreference.Value;
                m_MatchFeature.MinAgePreference = (int)i_MinPreference.Value;
                m_MatchFeature.Execute(i_MatchesListBox);
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void ResetUser()
        {
            m_UserLoggedIn = null;
        }

    }
}
