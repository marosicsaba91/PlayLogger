#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;  
using UnityEditor;
using UnityEngine;
using MUtility;
using Object = UnityEngine.Object;

namespace PlayLogging
{
public class PlayLogsWindow : EditorWindow
{
	TimeType _timeType = TimeType.RealTime;
	int _selectedLogIndex = -1;
	Vector2 _selectedScrollPos = Vector2.zero;
	LogStore _logStore;
	LogTag _filterTag;
	int _logTypeFilter = LogTypeHelper.all;

	GUITable<Log> _table;

	GUITable<Log> Table
	{
		get
		{
			if (_table == null)
				GenerateTable();
			return _table;
		}
	}


	enum TimeType
	{
		RealTime,
		GameTime,
		FixedTime,
		UnscaledTime,
		UnscaledFixedTime,
		FrameCount,
	}

	GUIStyle _centeredLabelStyle;

	GUIStyle CenteredLabelStyle
	{
		get
		{
			if (_centeredLabelStyle != null) return _centeredLabelStyle;
			_centeredLabelStyle = new GUIStyle(GUI.skin.label)
			{
				alignment = TextAnchor.MiddleCenter
			};
			return _centeredLabelStyle;
		}
	}

	[MenuItem("Tools/Play Logs")]
	static void ShowWindow()
	{
		var window = GetWindow<PlayLogsWindow>();
		window.titleContent = new GUIContent("Play Logs"); 
		window.Show();
	} 

	void OnGUI()
	{ 
		Rect headerRect = DrawHeader();
		var logsRect = new Rect(0, headerRect.yMax, position.width, position.height - headerRect.height);
		NoLogSetErrorMessage(logsRect);
		DrawObjectsTable(logsRect); 
	}

	void GenerateTable()
	{
		const int timeWidth = 70;

		var columns = new List<IColumn<Log>>
		{
			new LabelColumn<Log>(log => log.Index.ToString(), new ColumnInfo
			{
				titleGetter = () => _logStore.Logs.Count.ToString(),
				fixWidth = 50,
				headerAlignment = Alignment.Right,
				style = Alignment.Right.ToGUIStyle(),
			}),
			new LabelColumn<Log>(log => LogTypeEditorHelper.ToIcon(log.LogType), new ColumnInfo
			{
				title = string.Empty,
				fixWidth = 25,
				headerAlignment = Alignment.Center,
				style = Alignment.Center.ToGUIStyle(),
			}),
			new LabelColumn<Log>(log => log.RealTimeShortString, new ColumnInfo
			{
				title = "Time",
				fixWidth = timeWidth,
				visibleGetter = () => _timeType == TimeType.RealTime,
				headerAlignment = Alignment.Right,
				style = Alignment.Right.ToGUIStyle(),
			}),
			new LabelColumn<Log>(log => log.GameTimeShortString, new ColumnInfo
			{
				title = "Game T.",
				fixWidth = timeWidth,
				visibleGetter = () => _timeType == TimeType.GameTime,
				headerAlignment = Alignment.Right,
				style = Alignment.Right.ToGUIStyle(),
			}),
			new LabelColumn<Log>(log => log.FixedTimeShortString, new ColumnInfo
			{
				title = "Fixed T.",
				fixWidth = timeWidth,
				visibleGetter = () => _timeType == TimeType.FixedTime,
				headerAlignment = Alignment.Right,
				style = Alignment.Right.ToGUIStyle(),
			}),
			new LabelColumn<Log>(log => log.UnscaledTimeShortString, new ColumnInfo
			{
				title = "U. Time",
				fixWidth = timeWidth,
				visibleGetter = () => _timeType == TimeType.UnscaledTime,
				headerAlignment = Alignment.Right,
				style = Alignment.Right.ToGUIStyle(),
			}),
			new LabelColumn<Log>(log => log.FixedUnscaledTimeShortString, new ColumnInfo
			{
				title = "U. F. Time",
				fixWidth = timeWidth,
				visibleGetter = () => _timeType == TimeType.UnscaledFixedTime,
				headerAlignment = Alignment.Right,
				style = Alignment.Right.ToGUIStyle(),
			}),
			new LabelColumn<Log>(log => log.FrameCount.ToString(), new ColumnInfo
			{
				title = "Frame",
				fixWidth = timeWidth,
				visibleGetter = () => _timeType == TimeType.FrameCount,
				headerAlignment = Alignment.Right,
				style = Alignment.Right.ToGUIStyle(),
			}),
			new LabelColumn<Log>(
				log => new string(log.MessageAndContent.TakeWhile(c => c != '\n').ToArray()),
				new ColumnInfo
				{
					title = "Message & Content",
					fixWidth = 250,
					relativeWidthWeight = 1
				}),
			new LogSourceColumn(new ColumnInfo
			{
				title = "Source",
				fixWidth = 100,
				relativeWidthWeight = 0.1f,
			}),
			new LogTagsColumn(new ColumnInfo
			{
				title = "Tags",
				fixWidth = 100,
				relativeWidthWeight = 0.25f,
			})
		};

		_table = new GUITable<Log>(columns, this)
		{
			clickOnRow = OnClickOnRow,
			isRowHighlightedGetter = IsRowSelected,
			editedObjectGetter = GetEditedObject
		};
	}

	void OnClickOnRow(int index, Log log) => _selectedLogIndex = _selectedLogIndex == index ? -1 : index;
	bool IsRowSelected(int index, Log log) => index == _selectedLogIndex;

	Object GetEditedObject()
	{
		return _logStore;
	}

	void NoLogSetErrorMessage(Rect logsRect)
	{
		if (_logStore != null) return;

		GUI.color = EditorHelper.ErrorRedColor;
		EditorGUI.LabelField(logsRect, "Set up the target LogSet to use the Logs window", CenteredLabelStyle);
		GUI.color = Color.white;
	}

	void OnEnable()
	{
		wantsMouseMove = true; 
		if (_logStore != null)
			_logStore.LogsChanged += OnLogsChanged;
	}

	void OnDisable()
	{
		if (_logStore != null)
			_logStore.LogsChanged -= OnLogsChanged;
	}

	Rect DrawHeader()
	{
		const int logSetFieldWidth = 170;
		const int spacing = 1;
		const int dropdownWidth = 22;
		const int toggleWidth = 25;
		const int clearButtonWidth = 65;
		const int indexTypeWidth = 145;
		const int tagFieldWidth = 155;

		var header = new EditorWindowHeader();

		float fullWidth = position.width;
		const float usedWidth = logSetFieldWidth + spacing + indexTypeWidth + dropdownWidth +
		                        clearButtonWidth + (6 * toggleWidth) + tagFieldWidth;
		float extraWidth = fullWidth - usedWidth;
		header.SetupHeader(position);

		_logStore = header.DrawObjectField(logSetFieldWidth, _logStore, typeof(LogStore), LogSetChanged);

		if (_logStore == null)
			return header.FullHeaderPosition;

		header.Space(spacing);

		_timeType = header.DrawEnumDropdown(indexTypeWidth, _timeType);
		if (_logStore.HasLogs)
		{
			if (header.DrawButton(clearButtonWidth, "Clear"))
				_logStore.ClearLogs();
		}
		else if (_logStore.IsRestorable)
		{
			if (header.DrawButton(clearButtonWidth, "Restore"))
				_logStore.Restore();
		}
		else
			header.Space(clearButtonWidth);

		header.DrawMultiSelectPopupButton(
			dropdownWidth, new[]
			{
				new EditorWindowHeader.PopupElement(
					"Clear On Play",
					() => _logStore.ClearOnPlay,
					val => _logStore.ClearOnPlay = val),
				new EditorWindowHeader.PopupElement(
					"Clear On Build",
					() => _logStore.ClearOnBuild,
					val => _logStore.ClearOnBuild = val),
			});

		header.Space(extraWidth);

		_logTypeFilter = DrawFilterToggle(_logTypeFilter, LogType.PlayLog);
		_logTypeFilter = DrawFilterToggle(_logTypeFilter, LogType.UnityLog);
		_logTypeFilter = DrawFilterToggle(_logTypeFilter, LogType.UnityWarning);
		_logTypeFilter = DrawFilterToggle(_logTypeFilter, LogType.UnityError);
		_logTypeFilter = DrawFilterToggle(_logTypeFilter, LogType.UnityAssert);
		_logTypeFilter = DrawFilterToggle(_logTypeFilter, LogType.Exception);

		_filterTag = header.DrawObjectField(tagFieldWidth, _filterTag, typeof(LogTag));

		return header.FullHeaderPosition;

		int DrawFilterToggle(int filter, LogType logType)
		{
			bool isChecked = (filter & (int) logType) != 0;
			bool toggleResult = header.DrawToggleButton(toggleWidth, LogTypeEditorHelper.ToIcon(logType),
				isChecked);
			if (toggleResult != isChecked)
				return filter ^ (int) logType;
			return filter;
		}
	}


	void LogSetChanged(LogStore oldValue, LogStore newValue)
	{
		if (oldValue != null)
			oldValue.LogsChanged -= OnLogsChanged;
		if (newValue != null)
			newValue.LogsChanged += OnLogsChanged;
	}

	void OnLogsChanged()
	{
		Repaint();
	}

	void DrawObjectsTable(Rect logsRect)
	{
		if (_logStore == null || _logStore.Logs == null) return;
		bool isSelected = _selectedLogIndex >= 0 && _selectedLogIndex < _logStore.Logs.Count;

		Rect tableRect = isSelected ? DrawSelected(logsRect, _logStore.Logs[_selectedLogIndex]) : logsRect;

		IReadOnlyList<Log> filtered = _filterTag == null && _logTypeFilter == LogTypeHelper.all
			? _logStore.Logs
			: _logStore.Logs.Where(Check).ToList();

		Table.Draw(tableRect, filtered);
	}

	Rect DrawSelected(Rect position, Log log)
	{
		const int padding = 10;
		const int sliderWidth = 13;
		string messageText = log.Message;
		string contentText = log.ContentString(true);
		string stackTraceText = log.StackTrace;

		float messageTextHeight = LineHeight(messageText, padding);
		float contentTextHeight = LineHeight(contentText, padding);
		float stackTraceTextHeight = LineHeight(stackTraceText, padding);

		float requiredHeight = messageTextHeight + contentTextHeight + stackTraceTextHeight;
		float maxHeight = position.height / 2;
		float usedHeight = Mathf.Min(requiredHeight, maxHeight);
		float tableHeight = position.height - usedHeight;
		float width = usedHeight < requiredHeight ? position.width - sliderWidth : position.width;


		var selectedPosition = new Rect(position.x, position.y + tableHeight, position.width, usedHeight);
		var selectedArea = new Rect(0, 0, width, requiredHeight);

		_selectedScrollPos = GUI.BeginScrollView(selectedPosition, _selectedScrollPos, selectedArea);

		var textAreaStyle = new GUIStyle(GUI.skin.textField)
		{
			padding = new RectOffset(padding, padding, padding, padding),
		};
		textAreaStyle.active = textAreaStyle.normal;
		textAreaStyle.focused = textAreaStyle.normal;

		float y = 0;

		y = DrawTextArea(messageTextHeight, y, messageText);
		y = DrawTextArea(contentTextHeight, y, contentText);
		DrawTextArea(stackTraceTextHeight, y, stackTraceText);

		GUI.EndScrollView();

		return new Rect(position.x, position.y, position.width, tableHeight);


		float DrawTextArea(float height, float currentY, string text)
		{
			var rect = new Rect(0, currentY, selectedArea.width, height);
			GUI.TextArea(rect, text, textAreaStyle);
			currentY += height;

			return currentY;
		}
	}

	static float LineHeight(string text, int padding)
	{
		const float lineHeight = 15;
		int lineCount = LineCount(text);
		if (lineCount == 0) return 0;
		return Mathf.Max((lineCount * lineHeight) + (2 * padding), EditorGUIUtility.singleLineHeight);
	}

	static int LineCount(string text)
	{
		if (string.IsNullOrEmpty(text)) return 0;

		return text.Split('\n').Length;
	}

	public bool Check(Log log)
	{
		if (_filterTag != null &&
		    (log.Tags == null || !log.Tags.Contains(_filterTag))) return false;
		return ((int) log.LogType & _logTypeFilter) != 0;
	}
}
}

#endif