import { copyFile } from 'fs';

// Copy over files
copyFile('../data/statistics.json', 'public/statistics.json', (err) => {
    if (err) throw err;
    console.log('Statistics file copied successfully!');
});

copyFile('../data/parsed_messages.json', 'public/slack_messages.json', (err) => {
    if (err) throw err;
    console.log('Messages file copied successfully!');
});