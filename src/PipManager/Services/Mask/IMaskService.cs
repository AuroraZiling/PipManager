﻿using PipManager.Controls.Mask;
using PipManager.Models.Action;

namespace PipManager.Services.Mask;

public interface IMaskService
{
    public void SetMaskPresenter(MaskPresenter maskPresenter);

    public MaskPresenter GetMaskPresenter();

    public void Show(string message = "");

    public void Hide();
}