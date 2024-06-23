// Copyright 2010 Nejat Dilek  <imruon@gmail.com>
// Licensed under the terms of the GNU GPL, version 2
// http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace WiiGSC
{
    public partial class InfoForm : Form
    {
        private AppDomain appDomain;
        private ArrayList listZak;
        private ArrayList nameListZak;
#if !USEBASS
        protected IrrKlang.ISoundEngine irrKlangEngine;
        protected IrrKlang.ISound currentlyPlayingSound;
#endif


        public InfoForm(AppDomain appDomain)
        {
            this.appDomain = appDomain;

            this.InitializeComponent();
            InitializeMyComponent();
        }


        private void InfoForm_Load(object sender, EventArgs e)
        {
            string strFileToPlay = null;
#if !USEBASS
            irrKlangEngine = new IrrKlang.ISoundEngine();            
#else
            Un4seen.BassMOD.BassMOD.BASSMOD_Init(-1, 44100, Un4seen.BassMOD.BASSInit.BASS_DEVICE_DEFAULT);
#endif
            int randomZak = new Random().Next(this.listZak.Count);
            //strFileToPlay = (this.appDomain.BaseDirectory + @"\Muzak\") + ((string) this.listZak[randomZak]) + ".xm";
            strFileToPlay = (this.appDomain.BaseDirectory + @"\Muzak\Bakakalirim giden geminin ardindan.xm") ;
            this.lblZakCredits.Text = "Bakakalırım giden geminin ardından by WiiCrazy/I.R.on";
            playFile(strFileToPlay);
        }

        // plays filename selected in edit box
        void playFile(string musicPath)
        {
#if !USEBASS
            // stop currently playing sound
            if (currentlyPlayingSound != null) 
                currentlyPlayingSound.Stop();

            // start new sound
            currentlyPlayingSound = irrKlangEngine.Play2D(musicPath, true);
#else
            Un4seen.BassMOD.BassMOD.BASSMOD_MusicLoad(musicPath, 0, 0, Un4seen.BassMOD.BASSMusic.BASS_MUSIC_LOOP);
            Un4seen.BassMOD.BassMOD.BASSMOD_MusicPlay();
#endif

        }


        private void InitializeMyComponent()
        {
            string zak2 = null;
            string zak1 = null;
            this.listZak = new ArrayList();
            this.listZak.Add("jt_breez");
            this.listZak.Add("jt_breez");
            this.listZak.Add("jt_breez");
            this.listZak.Add("jt_mind");
            this.listZak.Add("jt_mind");
            this.listZak.Add("jt_mind");
            this.listZak.Add("jt_mind");
            this.listZak.Add("jt_1999");
            this.listZak.Add("jt_letgo");
            this.listZak.Add("jt_xmas");
            this.nameListZak = new ArrayList();
            zak1 = "Mountain Breeze by Jeroen Tel";
            zak2 = "In my life, in my mind by Jeroen Tel";
            this.nameListZak.Add(zak1);
            this.nameListZak.Add(zak1);
            this.nameListZak.Add(zak1);
            this.nameListZak.Add(zak2);
            this.nameListZak.Add(zak2);
            this.nameListZak.Add(zak2);
            this.nameListZak.Add(zak2);
            this.nameListZak.Add("1999 by Jeroen Tel");
            this.nameListZak.Add("Letting go by Jeroen Tel");
            this.nameListZak.Add("Merry Xmas & Funky99 by Jeroen Tel");
        }

        private void InfoForm_FormClosed(object sender, FormClosedEventArgs e)
        {
#if !USEBASS
            if (currentlyPlayingSound != null)
                currentlyPlayingSound.Stop();
#else
            if (Un4seen.BassMOD.BassMOD.BASSMOD_MusicIsActive()>0)
            {
                Un4seen.BassMOD.BassMOD.BASSMOD_MusicStop();
            }
            Un4seen.BassMOD.BassMOD.BASSMOD_Free();
#endif
        }

    }
}
