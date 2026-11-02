import { copyFile } from 'fs';

// Copy over files
copyFile('../data/statistics.json', 'src/assets/statistics.json', (err) => {
    if (err) throw err;
    console.log('Statistics file copied successfully!');
});

copyFile('../data/parsed_messages.json', 'src/assets/slack_messages.json', (err) => {
    if (err) throw err;
    console.log('Messages file copied successfully!');
});