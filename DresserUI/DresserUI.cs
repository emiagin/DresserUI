using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.Menus;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DresserUI
{
	internal class CustomPopup : IClickableMenu
	{
		private readonly string title;
		private ClickableTextureComponent closeButton;
		private Texture2D titleBackgroundTexture; // The texture for the title background

		private readonly List<Item> items; // Items to display
		private readonly int itemsPerRow;
		private readonly int itemSlotSize = 64; // Stardew's default inventory slot size is 64x64 pixels
		private readonly List<ClickableComponent> itemSlots;

		private int scrollAmount; // The amount of offset due to scrolling
		private int maxScroll; // The maximum scroll amount, which you'll calculate
		private readonly int pixelsPerScroll = 64; // How many pixels each scroll notch moves
		private int maxRows;
		private int gridPaddings = 0;

		private int[] paddingsBox = new int[4]; // top left bottom right
		private Rectangle avaliableArea;

		public CustomPopup(IModHelper helper, string title, List<Item> items) : base((Game1.viewport.Width - 800) / 2, (Game1.viewport.Height - 600) / 2, 800, 600)
		{
			// Load the title background texture
			this.titleBackgroundTexture = helper.ModContent.Load<Texture2D>("assets/titleBackground.png");

			UpdatePaddings();
			UpdateAvaliableArea();

			this.title = title;
			this.items = items;
			this.itemSlots = new List<ClickableComponent>();

			this.itemsPerRow = (int)(avaliableArea.Width / (itemSlotSize + gridPaddings));
			int visibleRows = (int)(avaliableArea.Height / (itemSlotSize + gridPaddings));
			maxRows = Math.Max(0, (this.items.Count + itemsPerRow - 1) / itemsPerRow);

			// Calculate positions for item slots
			for (int i = 0; i < Math.Min(this.items.Count, itemsPerRow * maxRows); i++)
			{
				int row = i / itemsPerRow;
				int column = i % itemsPerRow;
				int x = this.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder + (column * itemSlotSize);
				int y = this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + titleBackgroundTexture.Height + (row * itemSlotSize);

				if (row >= visibleRows + scrollAmount / itemSlotSize)
				{
					break;
				}

				ClickableComponent itemSlot = new ClickableComponent(new Rectangle(x, y, itemSlotSize, itemSlotSize), i.ToString());
				this.itemSlots.Add(itemSlot);
			}

			// Create a close button
			Texture2D texture = helper.ModContent.Load<Texture2D>("assets/closeButton.png");
			this.closeButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - 64 - 16, this.yPositionOnScreen + 16, 64, 64), texture, Rectangle.Empty, 1f);
			
			CalculateMaxScroll();
		}

		public override void receiveScrollWheelAction(int direction)
		{
			// 'direction' will be positive when scrolling up, negative when scrolling down
			if (direction > 0 && scrollAmount > 0)
			{
				scrollAmount = Math.Max(0, scrollAmount - pixelsPerScroll);
			}
			else if (direction < 0 && scrollAmount < maxScroll)
			{
				scrollAmount = Math.Min(maxScroll, scrollAmount + pixelsPerScroll);
			}
		}
		private void UpdatePaddings()
		{
			// top
			paddingsBox[0] = titleBackgroundTexture.Height + IClickableMenu.borderWidth + 5;
			// left
			paddingsBox[1] = IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder;
			// bottom
			paddingsBox[2] = IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 50;
			// right
			paddingsBox[3] = IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder;
		}
		private void UpdateAvaliableArea()
		{
			avaliableArea = new Rectangle(this.xPositionOnScreen + paddingsBox[1],      //x
										this.yPositionOnScreen + paddingsBox[0],        //y
										this.width - paddingsBox[3] - paddingsBox[1],   //width
										this.height - paddingsBox[2]);                  //height
		}
		private void CalculateMaxScroll()
		{
			// This needs to consider the actual drawable area, excluding the title background
			int drawableAreaHeight = this.height - titleBackgroundTexture.Height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder - 8;
			int totalGridHeight = maxRows * itemSlotSize;
			maxScroll = Math.Max(0, totalGridHeight - drawableAreaHeight);
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);

			// Draw the DEBUG before dialog box
			//Rectangle backgroundRect = new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height);
			//b.Draw(Game1.fadeToBlackRect, backgroundRect, Color.White * 0.7f);

			// Draw the dialog box
			Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true);

			// Draw the DEBUG inside dialog box
			b.Draw(Game1.fadeToBlackRect, avaliableArea, Color.Black * 0.7f);

			// Draw the title background
			Vector2 titleBackgroundPosition = new Vector2(this.xPositionOnScreen + (this.width - titleBackgroundTexture.Width) / 2, this.yPositionOnScreen + 20);
			b.Draw(titleBackgroundTexture, titleBackgroundPosition, Color.White);

			// Draw the title
			SpriteFont titleFont = Game1.dialogueFont;
			Vector2 titleSize = titleFont.MeasureString(this.title);
			b.DrawString(titleFont, this.title, new Vector2(this.xPositionOnScreen + (this.width - titleSize.X) / 2, this.yPositionOnScreen + 20 + (titleBackgroundTexture.Height - titleSize.Y) / 2), Game1.textColor);

			// Draw the close button
			this.closeButton.draw(b);

			// Begin drawing items
			for (int i = 0; i < this.items.Count; i++)
			{
				int row = i / itemsPerRow;
				int column = i % itemsPerRow;
				int x = this.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder + (column * itemSlotSize);
				int y = this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + (row * itemSlotSize) - scrollAmount;

				// Draw the item only if it's within the visible area
				if (y + itemSlotSize > this.yPositionOnScreen && y < this.yPositionOnScreen + this.height - itemSlotSize)
				{
					Item item = this.items[i];
					Vector2 location = new Vector2(x, y);

					// Draw item background
					b.Draw(Game1.menuTexture, location, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);

					// Draw the item sprite
					if (item != null)
					{
						item.drawInMenu(b, location, 1f);
					}
				}
			}
			// Draw mouse last so it's on top of everything
			this.drawMouse(b);

			// Update Info
			UpdatePaddings();
			UpdateAvaliableArea();
			CalculateMaxScroll();
		}
	}
}
